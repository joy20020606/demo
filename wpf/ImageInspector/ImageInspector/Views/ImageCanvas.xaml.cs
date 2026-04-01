using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ImageInspector.Models;
using ImageInspector.ViewModels;

namespace ImageInspector.Views;

public partial class ImageCanvas : UserControl
{
    private bool _isPanning;
    private bool _isDrawing;
    private Point _panStart;
    private Point _drawStart;
    private double _scale = 1.0;
    private Shape? _currentShape;
    private MainViewModel? _vm;

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(ImageCanvas),
            new PropertyMetadata(null, OnViewModelChanged));

    public MainViewModel? ViewModel
    {
        get => (MainViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public ImageCanvas()
    {
        InitializeComponent();
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var canvas = (ImageCanvas)d;
        if (e.OldValue is MainViewModel oldVm)
        {
            oldVm.PropertyChanged -= canvas.Vm_PropertyChanged;
            oldVm.Annotations.CollectionChanged -= canvas.Annotations_CollectionChanged;
        }
        if (e.NewValue is MainViewModel newVm)
        {
            canvas._vm = newVm;
            newVm.PropertyChanged += canvas.Vm_PropertyChanged;
            newVm.Annotations.CollectionChanged += canvas.Annotations_CollectionChanged;
        }
    }

    private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.DisplayImage))
        {
            UpdateImage();
        }
        else if (e.PropertyName == nameof(MainViewModel.SelectedAnnotation))
        {
            RefreshAnnotations();
        }
    }

    private void Annotations_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshAnnotations();
    }

    private void UpdateImage()
    {
        if (_vm?.DisplayImage == null)
        {
            MainImage.Source = null;
            PlaceholderText.Visibility = Visibility.Visible;
            AnnotationCanvas.Width = 0;
            AnnotationCanvas.Height = 0;
            return;
        }

        PlaceholderText.Visibility = Visibility.Collapsed;
        MainImage.Source = _vm.DisplayImage;

        double w = _vm.DisplayImage.PixelWidth * _scale;
        double h = _vm.DisplayImage.PixelHeight * _scale;
        MainImage.Width = w;
        MainImage.Height = h;
        AnnotationCanvas.Width = w;
        AnnotationCanvas.Height = h;

        RefreshAnnotations();
    }

    private void RefreshAnnotations()
    {
        AnnotationCanvas.Children.Clear();
        if (_vm == null) return;

        foreach (var ann in _vm.Annotations)
        {
            var shape = CreateShape(ann);
            AnnotationCanvas.Children.Add(shape);
        }
    }

    private Shape CreateShape(AnnotationViewModel ann)
    {
        double x = ann.X * _scale;
        double y = ann.Y * _scale;
        double w = ann.Width * _scale;
        double h = ann.Height * _scale;

        bool selected = ann.IsSelected || ann == _vm?.SelectedAnnotation;
        var stroke = selected ? Brushes.Yellow : new SolidColorBrush(ParseColor(ann.Color));
        double thickness = selected ? 2.5 : 1.5;

        Shape shape;
        if (ann.Shape == AnnotationShape.Ellipse)
        {
            shape = new Ellipse();
        }
        else
        {
            shape = new Rectangle();
        }

        shape.Stroke = stroke;
        shape.StrokeThickness = thickness;
        shape.Fill = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
        shape.Width = Math.Abs(w);
        shape.Height = Math.Abs(h);
        Canvas.SetLeft(shape, Math.Min(x, x + w));
        Canvas.SetTop(shape, Math.Min(y, y + h));
        shape.Tag = ann;

        shape.MouseLeftButtonDown += (s, e) =>
        {
            if (_vm != null)
                _vm.SelectedAnnotation = ann;
            e.Handled = true;
        };

        if (!string.IsNullOrEmpty(ann.Label))
        {
            shape.ToolTip = ann.Label;
        }

        return shape;
    }

    private static Color ParseColor(string hex)
    {
        try { return (Color)ColorConverter.ConvertFromString(hex); }
        catch { return Colors.Red; }
    }

    private void ScrollView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            double factor = e.Delta > 0 ? 1.15 : 1.0 / 1.15;
            _scale = Math.Clamp(_scale * factor, 0.05, 32.0);
            UpdateImage();
        }
    }

    private void AnnotationCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm == null || !_vm.HasImage) return;

        if (Keyboard.Modifiers == ModifierKeys.Alt)
        {
            _isPanning = true;
            _panStart = e.GetPosition(ScrollView);
            AnnotationCanvas.CaptureMouse();
            return;
        }

        _isDrawing = true;
        _drawStart = e.GetPosition(AnnotationCanvas);
        AnnotationCanvas.CaptureMouse();

        _currentShape = _vm.SelectedShape == AnnotationShape.Ellipse
            ? new Ellipse() as Shape
            : new Rectangle();

        _currentShape.Stroke = Brushes.Lime;
        _currentShape.StrokeThickness = 1.5;
        _currentShape.StrokeDashArray = new DoubleCollection { 4, 2 };
        _currentShape.Fill = new SolidColorBrush(Color.FromArgb(20, 0, 255, 0));
        Canvas.SetLeft(_currentShape, _drawStart.X);
        Canvas.SetTop(_currentShape, _drawStart.Y);
        AnnotationCanvas.Children.Add(_currentShape);
    }

    private void AnnotationCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isPanning)
        {
            var pos = e.GetPosition(ScrollView);
            ScrollView.ScrollToHorizontalOffset(ScrollView.HorizontalOffset - (pos.X - _panStart.X));
            ScrollView.ScrollToVerticalOffset(ScrollView.VerticalOffset - (pos.Y - _panStart.Y));
            _panStart = pos;
            return;
        }

        if (!_isDrawing || _currentShape == null) return;

        var cur = e.GetPosition(AnnotationCanvas);
        double x = Math.Min(cur.X, _drawStart.X);
        double y = Math.Min(cur.Y, _drawStart.Y);
        double w = Math.Abs(cur.X - _drawStart.X);
        double h = Math.Abs(cur.Y - _drawStart.Y);

        Canvas.SetLeft(_currentShape, x);
        Canvas.SetTop(_currentShape, y);
        _currentShape.Width = w;
        _currentShape.Height = h;
    }

    private void AnnotationCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        AnnotationCanvas.ReleaseMouseCapture();

        if (_isPanning)
        {
            _isPanning = false;
            return;
        }

        if (!_isDrawing || _currentShape == null || _vm == null)
        {
            _isDrawing = false;
            return;
        }

        _isDrawing = false;
        var cur = e.GetPosition(AnnotationCanvas);
        double w = Math.Abs(cur.X - _drawStart.X);
        double h = Math.Abs(cur.Y - _drawStart.Y);

        if (w > 5 && h > 5)
        {
            var annotation = new Annotation
            {
                Shape = _vm.SelectedShape,
                X = Math.Min(cur.X, _drawStart.X) / _scale,
                Y = Math.Min(cur.Y, _drawStart.Y) / _scale,
                Width = w / _scale,
                Height = h / _scale,
                Label = string.Empty,
                Color = "#FF0000"
            };
            _vm.AddAnnotation(annotation);
        }

        if (_currentShape != null)
        {
            AnnotationCanvas.Children.Remove(_currentShape);
            _currentShape = null;
        }

        RefreshAnnotations();
    }
}
