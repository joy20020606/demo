using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MultiplexAnalyzer.Hmi.ViewModels;

namespace MultiplexAnalyzer.Hmi.Controls;

public class PlateMapControl : Control
{
    private const double LabelGutter = 30;
    private const double Inset = 12;
    private const double MinZoom = 1;
    private const double MaxZoom = 6;
    private const double DragThreshold = 6;
    private const double WheelZoomFactor = 1.15;
    private const int RampSteps = 9;

    private readonly SolidColorBrush[] rampCache = new SolidColorBrush[RampSteps];
    private (Color Low, Color High) rampKey;

    private Point mouseDownPoint;
    private Point lastMousePoint;
    private bool isMouseDown;
    private bool isMouseDragging;

    static PlateMapControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PlateMapControl),
            new FrameworkPropertyMetadata(typeof(PlateMapControl)));
    }

    public PlateMapControl()
    {
        IsManipulationEnabled = true;
        ClipToBounds = true;
    }

    public static readonly DependencyProperty WellsProperty = Register(
        nameof(Wells), typeof(IReadOnlyList<WellViewModel>), null);

    public static readonly DependencyProperty RowsProperty = Register(nameof(Rows), typeof(int), 8);

    public static readonly DependencyProperty ColumnsProperty = Register(nameof(Columns), typeof(int), 12);

    public static readonly DependencyProperty SelectedWellProperty = DependencyProperty.Register(
        nameof(SelectedWell),
        typeof(WellViewModel),
        typeof(PlateMapControl),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
        nameof(Zoom),
        typeof(double),
        typeof(PlateMapControl),
        new FrameworkPropertyMetadata(
            1d,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender,
            OnZoomChanged,
            CoerceZoom));

    public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register(
        nameof(OffsetX),
        typeof(double),
        typeof(PlateMapControl),
        new FrameworkPropertyMetadata(
            0d,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register(
        nameof(OffsetY),
        typeof(double),
        typeof(PlateMapControl),
        new FrameworkPropertyMetadata(
            0d,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty PlateBrushProperty = Register(nameof(PlateBrush), typeof(Brush), null);

    public static readonly DependencyProperty PlateBorderBrushProperty = Register(nameof(PlateBorderBrush), typeof(Brush), null);

    public static readonly DependencyProperty LabelBrushProperty = Register(nameof(LabelBrush), typeof(Brush), null);

    public static readonly DependencyProperty WellLowBrushProperty = Register(nameof(WellLowBrush), typeof(Brush), null);

    public static readonly DependencyProperty WellHighBrushProperty = Register(nameof(WellHighBrush), typeof(Brush), null);

    public static readonly DependencyProperty SelectionBrushProperty = Register(nameof(SelectionBrush), typeof(Brush), null);

    public IReadOnlyList<WellViewModel>? Wells
    {
        get => (IReadOnlyList<WellViewModel>?)GetValue(WellsProperty);
        set => SetValue(WellsProperty, value);
    }

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public WellViewModel? SelectedWell
    {
        get => (WellViewModel?)GetValue(SelectedWellProperty);
        set => SetValue(SelectedWellProperty, value);
    }

    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public double OffsetX
    {
        get => (double)GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    public double OffsetY
    {
        get => (double)GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    public Brush? PlateBrush
    {
        get => (Brush?)GetValue(PlateBrushProperty);
        set => SetValue(PlateBrushProperty, value);
    }

    public Brush? PlateBorderBrush
    {
        get => (Brush?)GetValue(PlateBorderBrushProperty);
        set => SetValue(PlateBorderBrushProperty, value);
    }

    public Brush? LabelBrush
    {
        get => (Brush?)GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    public Brush? WellLowBrush
    {
        get => (Brush?)GetValue(WellLowBrushProperty);
        set => SetValue(WellLowBrushProperty, value);
    }

    public Brush? WellHighBrush
    {
        get => (Brush?)GetValue(WellHighBrushProperty);
        set => SetValue(WellHighBrushProperty, value);
    }

    public Brush? SelectionBrush
    {
        get => (Brush?)GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    private double CellSize
    {
        get
        {
            var width = ActualWidth - LabelGutter - (Inset * 2);
            var height = ActualHeight - LabelGutter - (Inset * 2);
            return Math.Max(1, Math.Min(width / Columns, height / Rows));
        }
    }

    private Point PlateOrigin => new(LabelGutter + Inset, LabelGutter + Inset);

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Background ?? Brushes.Transparent, null, new Rect(RenderSize));

        if (ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        var cell = CellSize;
        var origin = PlateOrigin;
        var radius = cell * 0.38;

        dc.PushTransform(new MatrixTransform(Zoom, 0, 0, Zoom, OffsetX, OffsetY));

        DrawPlateBody(dc, origin, cell);
        DrawLabels(dc, origin, cell);
        DrawWells(dc, origin, cell, radius);
        DrawSelection(dc, origin, cell, radius);

        dc.Pop();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        ClampOffsets();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        ZoomAt(e.Delta > 0 ? WheelZoomFactor : 1 / WheelZoomFactor, e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        isMouseDown = true;
        isMouseDragging = false;
        mouseDownPoint = lastMousePoint = e.GetPosition(this);
        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!isMouseDown)
        {
            return;
        }

        var position = e.GetPosition(this);

        if (!isMouseDragging && (position - mouseDownPoint).Length > DragThreshold)
        {
            isMouseDragging = true;
        }

        if (isMouseDragging)
        {
            Pan(position.X - lastMousePoint.X, position.Y - lastMousePoint.Y);
        }

        lastMousePoint = position;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!isMouseDown)
        {
            return;
        }

        isMouseDown = false;
        ReleaseMouseCapture();

        if (!isMouseDragging)
        {
            SelectedWell = WellAt(e.GetPosition(this));
        }

        e.Handled = true;
    }

    protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
    {
        base.OnManipulationStarting(e);
        e.ManipulationContainer = this;
        e.Mode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        e.Handled = true;
    }

    protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
    {
        base.OnManipulationDelta(e);

        var delta = e.DeltaManipulation;

        if (Math.Abs(delta.Scale.X - 1) > 0.001)
        {
            ZoomAt(delta.Scale.X, e.ManipulationOrigin);
        }

        Pan(delta.Translation.X, delta.Translation.Y);
        e.Handled = true;
    }

    protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
    {
        base.OnManipulationCompleted(e);

        var total = e.TotalManipulation;
        var isTap = total.Translation.Length < DragThreshold && Math.Abs(total.Scale.X - 1) < 0.05;

        if (isTap)
        {
            SelectedWell = WellAt(e.ManipulationOrigin);
        }

        e.Handled = true;
    }

    private void ZoomAt(double factor, Point screenPoint)
    {
        var target = Math.Clamp(Zoom * factor, MinZoom, MaxZoom);
        var applied = target / Zoom;

        if (Math.Abs(applied - 1) < 0.0001)
        {
            return;
        }

        OffsetX = screenPoint.X - ((screenPoint.X - OffsetX) * applied);
        OffsetY = screenPoint.Y - ((screenPoint.Y - OffsetY) * applied);
        Zoom = target;
        ClampOffsets();
    }

    private void Pan(double dx, double dy)
    {
        OffsetX += dx;
        OffsetY += dy;
        ClampOffsets();
    }

    private void ClampOffsets()
    {
        var minX = ActualWidth - (ActualWidth * Zoom);
        var minY = ActualHeight - (ActualHeight * Zoom);
        var clampedX = Math.Clamp(OffsetX, Math.Min(minX, 0), 0);
        var clampedY = Math.Clamp(OffsetY, Math.Min(minY, 0), 0);

        if (clampedX != OffsetX)
        {
            OffsetX = clampedX;
        }

        if (clampedY != OffsetY)
        {
            OffsetY = clampedY;
        }
    }

    private WellViewModel? WellAt(Point screenPoint)
    {
        if (Wells is null)
        {
            return null;
        }

        var world = new Point((screenPoint.X - OffsetX) / Zoom, (screenPoint.Y - OffsetY) / Zoom);
        var cell = CellSize;
        var origin = PlateOrigin;
        var column = (int)Math.Floor((world.X - origin.X) / cell);
        var row = (int)Math.Floor((world.Y - origin.Y) / cell);

        if (row < 0 || row >= Rows || column < 0 || column >= Columns)
        {
            return null;
        }

        return Wells.FirstOrDefault(well => well.Row == row && well.Column == column);
    }

    private void DrawPlateBody(DrawingContext dc, Point origin, double cell)
    {
        var rect = new Rect(
            origin.X - (Inset / 2),
            origin.Y - (Inset / 2),
            (Columns * cell) + Inset,
            (Rows * cell) + Inset);

        var pen = new Pen(PlateBorderBrush ?? Brushes.Gray, 1.5 / Zoom);
        pen.Freeze();
        dc.DrawRoundedRectangle(PlateBrush ?? Brushes.Transparent, pen, rect, 10, 10);
    }

    private void DrawLabels(DrawingContext dc, Point origin, double cell)
    {
        var brush = LabelBrush ?? Brushes.Gray;
        var typeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        for (var column = 0; column < Columns; column++)
        {
            var text = CreateText((column + 1).ToString(CultureInfo.InvariantCulture), typeface, 13, brush, dpi);
            var x = origin.X + (column * cell) + (cell / 2) - (text.Width / 2);
            var y = origin.Y - Inset - text.Height - 4;
            dc.DrawText(text, new Point(x, y));
        }

        for (var row = 0; row < Rows; row++)
        {
            var text = CreateText(((char)('A' + row)).ToString(), typeface, 13, brush, dpi);
            var x = origin.X - Inset - text.Width - 6;
            var y = origin.Y + (row * cell) + (cell / 2) - (text.Height / 2);
            dc.DrawText(text, new Point(x, y));
        }
    }

    private void DrawWells(DrawingContext dc, Point origin, double cell, double radius)
    {
        if (Wells is null)
        {
            return;
        }

        EnsureRamp();

        var pen = new Pen(PlateBorderBrush ?? Brushes.Gray, 1 / Zoom);
        pen.Freeze();

        var showLabels = Zoom >= 2.2;
        var typeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var labelBrush = LabelBrush ?? Brushes.Gray;

        foreach (var well in Wells)
        {
            var centre = WellCentre(well, origin, cell);
            var step = (int)Math.Round(well.Summary * (RampSteps - 1));
            dc.DrawEllipse(rampCache[step], pen, centre, radius, radius);

            if (showLabels)
            {
                var text = CreateText(well.Label, typeface, 9 / Zoom * 2, labelBrush, dpi);
                dc.DrawText(text, new Point(centre.X - (text.Width / 2), centre.Y + radius + 2));
            }
        }
    }

    private void DrawSelection(DrawingContext dc, Point origin, double cell, double radius)
    {
        if (SelectedWell is null)
        {
            return;
        }

        var centre = WellCentre(SelectedWell, origin, cell);
        var pen = new Pen(SelectionBrush ?? Brushes.DodgerBlue, 3 / Zoom);
        pen.Freeze();
        var ringRadius = radius + (5 / Zoom);
        dc.DrawEllipse(null, pen, centre, ringRadius, ringRadius);
    }

    private static Point WellCentre(WellViewModel well, Point origin, double cell)
        => new(origin.X + (well.Column * cell) + (cell / 2), origin.Y + (well.Row * cell) + (cell / 2));

    private void EnsureRamp()
    {
        var low = (WellLowBrush as SolidColorBrush)?.Color ?? Colors.LightGray;
        var high = (WellHighBrush as SolidColorBrush)?.Color ?? Colors.Red;

        if (rampKey == (low, high) && rampCache[0] is not null)
        {
            return;
        }

        rampKey = (low, high);

        for (var index = 0; index < RampSteps; index++)
        {
            var t = (double)index / (RampSteps - 1);
            var brush = new SolidColorBrush(Lerp(low, high, t));
            brush.Freeze();
            rampCache[index] = brush;
        }
    }

    private static Color Lerp(Color a, Color b, double t) => Color.FromArgb(
        (byte)(a.A + ((b.A - a.A) * t)),
        (byte)(a.R + ((b.R - a.R) * t)),
        (byte)(a.G + ((b.G - a.G) * t)),
        (byte)(a.B + ((b.B - a.B) * t)));

    private static FormattedText CreateText(string text, Typeface typeface, double size, Brush brush, double dpi)
        => new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, size, brush, dpi);

    private static object CoerceZoom(DependencyObject d, object baseValue)
        => Math.Clamp((double)baseValue, MinZoom, MaxZoom);

    private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((PlateMapControl)d).ClampOffsets();

    private static DependencyProperty Register(string name, Type type, object? defaultValue)
        => DependencyProperty.Register(
            name,
            type,
            typeof(PlateMapControl),
            new FrameworkPropertyMetadata(defaultValue, FrameworkPropertyMetadataOptions.AffectsRender));
}
