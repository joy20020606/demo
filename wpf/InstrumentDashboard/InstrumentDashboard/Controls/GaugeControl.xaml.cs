using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InstrumentDashboard.Controls;

public partial class GaugeControl : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(100.0, OnValueChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(GaugeControl),
            new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(GaugeControl),
            new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(nameof(Format), typeof(string), typeof(GaugeControl),
            new PropertyMetadata("F2", OnValueChanged));

    public static readonly DependencyProperty GaugeColorProperty =
        DependencyProperty.Register(nameof(GaugeColor), typeof(Color), typeof(GaugeControl),
            new PropertyMetadata(Colors.DodgerBlue, OnValueChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public Color GaugeColor
    {
        get => (Color)GetValue(GaugeColorProperty);
        set => SetValue(GaugeColorProperty, value);
    }

    public GaugeControl()
    {
        InitializeComponent();
        Loaded += (_, _) => Draw();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((GaugeControl)d).Draw();

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((GaugeControl)d).UpdateLabels();

    private void UpdateLabels()
    {
        LabelText.Text = Label;
        UnitText.Text = Unit;
    }

    private void Draw()
    {
        GaugeCanvas.Children.Clear();

        const double cx = 80, cy = 80, radius = 68, strokeWidth = 12;
        const double startAngle = 220, sweepMax = 280;

        DrawArc(cx, cy, radius, strokeWidth, startAngle, sweepMax, Color.FromRgb(50, 50, 60));

        var range = Maximum - Minimum;
        var fraction = range == 0 ? 0 : Math.Clamp((Value - Minimum) / range, 0, 1);
        var sweep = fraction * sweepMax;

        var fillColor = fraction < 0.6 ? GaugeColor
            : fraction < 0.85 ? Colors.Orange
            : Colors.OrangeRed;

        if (sweep > 0)
            DrawArc(cx, cy, radius, strokeWidth, startAngle, sweep, fillColor);

        DrawTicks(cx, cy, radius);

        ValueText.Text = Value.ToString(Format);
        LabelText.Text = Label;
        UnitText.Text = Unit;
    }

    private void DrawArc(double cx, double cy, double r, double strokeWidth,
        double startDeg, double sweepDeg, Color color)
    {
        var startRad = startDeg * Math.PI / 180.0;
        var endRad = (startDeg + sweepDeg) * Math.PI / 180.0;

        var startPoint = new Point(cx + r * Math.Cos(startRad), cy + r * Math.Sin(startRad));
        var endPoint = new Point(cx + r * Math.Cos(endRad), cy + r * Math.Sin(endRad));
        var largeArc = sweepDeg > 180;

        var figure = new PathFigure { StartPoint = startPoint };
        figure.Segments.Add(new ArcSegment(endPoint,
            new Size(r, r), 0, largeArc, SweepDirection.Clockwise, true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);

        var path = new Path
        {
            Data = geometry,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = strokeWidth,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };

        GaugeCanvas.Children.Add(path);
    }

    private void DrawTicks(double cx, double cy, double r)
    {
        const double startAngle = 220;
        const double sweepMax = 280;
        const int majorCount = 5;

        for (int i = 0; i <= majorCount; i++)
        {
            var angle = (startAngle + sweepMax * i / majorCount) * Math.PI / 180.0;
            var innerR = r - 18;
            var outerR = r - 8;

            var line = new Line
            {
                X1 = cx + innerR * Math.Cos(angle),
                Y1 = cy + innerR * Math.Sin(angle),
                X2 = cx + outerR * Math.Cos(angle),
                Y2 = cy + outerR * Math.Sin(angle),
                Stroke = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                StrokeThickness = 2
            };

            GaugeCanvas.Children.Add(line);
        }
    }
}
