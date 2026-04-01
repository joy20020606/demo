using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageInspector.Models;

namespace ImageInspector.ViewModels;

public partial class AnnotationViewModel : ObservableObject
{
    private readonly Annotation _model;

    public AnnotationViewModel(Annotation model)
    {
        _model = model;
    }

    public Guid Id => _model.Id;

    public AnnotationShape Shape
    {
        get => _model.Shape;
        set { _model.Shape = value; OnPropertyChanged(); }
    }

    public double X
    {
        get => _model.X;
        set { _model.X = value; OnPropertyChanged(); }
    }

    public double Y
    {
        get => _model.Y;
        set { _model.Y = value; OnPropertyChanged(); }
    }

    public double Width
    {
        get => _model.Width;
        set { _model.Width = value; OnPropertyChanged(); }
    }

    public double Height
    {
        get => _model.Height;
        set { _model.Height = value; OnPropertyChanged(); }
    }

    [ObservableProperty]
    private string _label = string.Empty;

    partial void OnLabelChanged(string value) => _model.Label = value;

    [ObservableProperty]
    private string _color = "#FF0000";

    partial void OnColorChanged(string value) => _model.Color = value;

    [ObservableProperty]
    private bool _isSelected;

    public string PositionText => $"({_model.X:F0}, {_model.Y:F0})";

    public Annotation ToModel()
    {
        _model.Label = Label;
        _model.Color = Color;
        return _model;
    }

    public static AnnotationViewModel FromModel(Annotation model)
    {
        var vm = new AnnotationViewModel(model);
        vm.Label = model.Label;
        vm.Color = model.Color;
        vm.IsSelected = model.IsSelected;
        return vm;
    }
}
