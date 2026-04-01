using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WaferMapViewer.Models;
using WaferMapViewer.Services;

namespace WaferMapViewer.Controls;

public class WaferMapControl : FrameworkElement
{
    private readonly VisualCollection _visuals;
    private DrawingVisual _backgroundVisual = new();
    private DrawingVisual _dieVisual = new();
    private DrawingVisual _highlightVisual = new();
    private DrawingVisual _overlayVisual = new();

    public static readonly DependencyProperty WaferMapProperty =
        DependencyProperty.Register(nameof(WaferMap), typeof(WaferMap), typeof(WaferMapControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnWaferMapChanged));

    public static readonly DependencyProperty ColorMapNameProperty =
        DependencyProperty.Register(nameof(ColorMapName), typeof(string), typeof(WaferMapControl),
            new FrameworkPropertyMetadata("Standard", FrameworkPropertyMetadataOptions.AffectsRender, OnColorMapChanged));

    public static readonly DependencyProperty HighlightResultProperty =
        DependencyProperty.Register(nameof(HighlightResult), typeof(InspectionResult?), typeof(WaferMapControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnHighlightChanged));

    public static readonly DependencyProperty SelectedDieProperty =
        DependencyProperty.Register(nameof(SelectedDie), typeof(DieInfo), typeof(WaferMapControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnSelectedDieChanged));

    public static readonly RoutedEvent DieClickedEvent =
        EventManager.RegisterRoutedEvent(nameof(DieClicked), RoutingStrategy.Bubble,
            typeof(DieClickedEventHandler), typeof(WaferMapControl));

    public WaferMap? WaferMap
    {
        get => (WaferMap?)GetValue(WaferMapProperty);
        set => SetValue(WaferMapProperty, value);
    }

    public string ColorMapName
    {
        get => (string)GetValue(ColorMapNameProperty);
        set => SetValue(ColorMapNameProperty, value);
    }

    public InspectionResult? HighlightResult
    {
        get => (InspectionResult?)GetValue(HighlightResultProperty);
        set => SetValue(HighlightResultProperty, value);
    }

    public DieInfo? SelectedDie
    {
        get => (DieInfo?)GetValue(SelectedDieProperty);
        set => SetValue(SelectedDieProperty, value);
    }

    public event DieClickedEventHandler DieClicked
    {
        add => AddHandler(DieClickedEvent, value);
        remove => RemoveHandler(DieClickedEvent, value);
    }

    private IColorMap _colorMap = ColorMapFactory.Create("Standard");

    public WaferMapControl()
    {
        _visuals = new VisualCollection(this);
        _visuals.Add(_backgroundVisual);
        _visuals.Add(_dieVisual);
        _visuals.Add(_highlightVisual);
        _visuals.Add(_overlayVisual);
        ClipToBounds = true;
    }

    protected override int VisualChildrenCount => _visuals.Count;
    protected override Visual GetVisualChild(int index) => _visuals[index];

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        Redraw();
    }

    private static void OnWaferMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((WaferMapControl)d).Redraw();

    private static void OnColorMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (WaferMapControl)d;
        ctrl._colorMap = ColorMapFactory.Create((string)e.NewValue);
        ctrl.Redraw();
    }

    private static void OnHighlightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((WaferMapControl)d).DrawHighlight();

    private static void OnSelectedDieChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((WaferMapControl)d).DrawOverlay();

    private void Redraw()
    {
        DrawBackground();
        DrawDies();
        DrawHighlight();
        DrawOverlay();
    }

    private (double dieW, double dieH, double offsetX, double offsetY) GetLayout()
    {
        var wafer = WaferMap;
        if (wafer == null || ActualWidth <= 0 || ActualHeight <= 0)
            return (0, 0, 0, 0);

        double dieW = ActualWidth / wafer.Cols;
        double dieH = ActualHeight / wafer.Rows;
        double size = Math.Min(dieW, dieH);
        double offsetX = (ActualWidth - size * wafer.Cols) / 2;
        double offsetY = (ActualHeight - size * wafer.Rows) / 2;
        return (size, size, offsetX, offsetY);
    }

    private void DrawBackground()
    {
        using var dc = _backgroundVisual.RenderOpen();
        dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x2E)), null,
            new Rect(0, 0, ActualWidth, ActualHeight));

        var wafer = WaferMap;
        if (wafer == null) return;

        var (dieW, dieH, offsetX, offsetY) = GetLayout();
        double cx = offsetX + dieW * wafer.Cols / 2;
        double cy = offsetY + dieH * wafer.Rows / 2;
        double rx = dieW * wafer.Cols / 2;
        double ry = dieH * wafer.Rows / 2;
        double r = Math.Min(rx, ry);

        var waferBrush = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x3A));
        var waferPen = new Pen(new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x90)), 2);
        dc.DrawEllipse(waferBrush, waferPen, new Point(cx, cy), r + dieW * 0.6, r + dieH * 0.6);
    }

    private void DrawDies()
    {
        using var dc = _dieVisual.RenderOpen();
        var wafer = WaferMap;
        if (wafer == null) return;

        var (dieW, dieH, offsetX, offsetY) = GetLayout();
        double gap = Math.Max(1, Math.Min(dieW, dieH) * 0.06);

        foreach (var die in wafer.Dies)
        {
            var color = _colorMap.GetColor(die.Result);
            var brush = new SolidColorBrush(color);
            var rect = new Rect(
                offsetX + die.Col * dieW + gap / 2,
                offsetY + die.Row * dieH + gap / 2,
                dieW - gap,
                dieH - gap);
            dc.DrawRectangle(brush, null, rect);
        }
    }

    private void DrawHighlight()
    {
        using var dc = _highlightVisual.RenderOpen();
        var wafer = WaferMap;
        if (wafer == null || HighlightResult == null) return;

        var (dieW, dieH, offsetX, offsetY) = GetLayout();
        double gap = Math.Max(1, Math.Min(dieW, dieH) * 0.06);

        var dimBrush = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));
        var glowPen = new Pen(new SolidColorBrush(Colors.White), 1.5);

        foreach (var die in wafer.Dies)
        {
            var rect = new Rect(
                offsetX + die.Col * dieW + gap / 2,
                offsetY + die.Row * dieH + gap / 2,
                dieW - gap,
                dieH - gap);

            if (die.Result != HighlightResult.Value)
                dc.DrawRectangle(dimBrush, null, rect);
            else
                dc.DrawRectangle(null, glowPen, rect);
        }
    }

    private void DrawOverlay()
    {
        using var dc = _overlayVisual.RenderOpen();
        var die = SelectedDie;
        var wafer = WaferMap;
        if (die == null || wafer == null) return;

        var (dieW, dieH, offsetX, offsetY) = GetLayout();
        double gap = Math.Max(1, Math.Min(dieW, dieH) * 0.06);

        var selectionPen = new Pen(new SolidColorBrush(Colors.Cyan), 2);
        var rect = new Rect(
            offsetX + die.Col * dieW + gap / 2,
            offsetY + die.Row * dieH + gap / 2,
            dieW - gap,
            dieH - gap);
        dc.DrawRectangle(null, selectionPen, rect);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        var wafer = WaferMap;
        if (wafer == null) return;

        var (dieW, dieH, offsetX, offsetY) = GetLayout();
        var pos = e.GetPosition(this);
        int col = (int)((pos.X - offsetX) / dieW);
        int row = (int)((pos.Y - offsetY) / dieH);

        var die = wafer.GetDie(row, col);
        if (die != null)
        {
            var args = new DieClickedEventArgs(DieClickedEvent, this, die);
            RaiseEvent(args);
        }
    }
}

public delegate void DieClickedEventHandler(object sender, DieClickedEventArgs e);

public class DieClickedEventArgs : RoutedEventArgs
{
    public DieInfo Die { get; }
    public DieClickedEventArgs(RoutedEvent routedEvent, object source, DieInfo die)
        : base(routedEvent, source)
    {
        Die = die;
    }
}
