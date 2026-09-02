using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MultiplexAnalyzer.Hmi.Controls;

public class ProgressRing : ContentControl
{
    static ProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(typeof(ProgressRing)));
    }

    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
        nameof(Progress),
        typeof(double),
        typeof(ProgressRing),
        new FrameworkPropertyMetadata(0d, OnProgressChanged, CoerceProgress));

    public static readonly DependencyProperty RingThicknessProperty = DependencyProperty.Register(
        nameof(RingThickness),
        typeof(double),
        typeof(ProgressRing),
        new FrameworkPropertyMetadata(16d, OnVisualChanged));

    private static readonly DependencyProperty AnimatedProgressProperty = DependencyProperty.Register(
        "AnimatedProgress",
        typeof(double),
        typeof(ProgressRing),
        new PropertyMetadata(0d, OnVisualChanged));

    private static readonly DependencyPropertyKey ArcGeometryPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ArcGeometry),
            typeof(Geometry),
            typeof(ProgressRing),
            new PropertyMetadata(Geometry.Empty));

    public static readonly DependencyProperty ArcGeometryProperty = ArcGeometryPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey TrackGeometryPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(TrackGeometry),
            typeof(Geometry),
            typeof(ProgressRing),
            new PropertyMetadata(Geometry.Empty));

    public static readonly DependencyProperty TrackGeometryProperty = TrackGeometryPropertyKey.DependencyProperty;

    public Geometry TrackGeometry => (Geometry)GetValue(TrackGeometryProperty);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public double RingThickness
    {
        get => (double)GetValue(RingThicknessProperty);
        set => SetValue(RingThicknessProperty, value);
    }

    public Geometry ArcGeometry => (Geometry)GetValue(ArcGeometryProperty);

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateGeometry();
    }

    private static object CoerceProgress(DependencyObject d, object baseValue)
        => Math.Clamp((double)baseValue, 0d, 1d);

    private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ring = (ProgressRing)d;
        var animation = new DoubleAnimation
        {
            From = (double)e.OldValue,
            To = (double)e.NewValue,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        ring.BeginAnimation(AnimatedProgressProperty, animation);
    }

    private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ProgressRing)d).UpdateGeometry();

    private void UpdateGeometry()
    {
        var side = Math.Min(ActualWidth, ActualHeight);
        var radius = (side - RingThickness) / 2;
        var fraction = (double)GetValue(AnimatedProgressProperty);

        if (radius <= 0)
        {
            SetValue(TrackGeometryPropertyKey, Geometry.Empty);
            SetValue(ArcGeometryPropertyKey, Geometry.Empty);
            return;
        }

        var centre = new Point(ActualWidth / 2, ActualHeight / 2);

        var track = new EllipseGeometry(centre, radius, radius);
        track.Freeze();
        SetValue(TrackGeometryPropertyKey, track);

        if (fraction <= 0)
        {
            SetValue(ArcGeometryPropertyKey, Geometry.Empty);
            return;
        }

        var degrees = Math.Min(fraction, 0.9999) * 360;
        var radians = degrees * Math.PI / 180;

        var start = new Point(centre.X, centre.Y - radius);
        var end = new Point(
            centre.X + (radius * Math.Sin(radians)),
            centre.Y - (radius * Math.Cos(radians)));

        var figure = new PathFigure
        {
            StartPoint = start,
            IsClosed = false,
            IsFilled = false
        };

        figure.Segments.Add(new ArcSegment(
            end,
            new Size(radius, radius),
            0,
            degrees > 180,
            SweepDirection.Clockwise,
            true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        geometry.Freeze();

        SetValue(ArcGeometryPropertyKey, geometry);
    }
}
