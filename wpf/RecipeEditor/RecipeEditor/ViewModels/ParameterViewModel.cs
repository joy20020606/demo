using CommunityToolkit.Mvvm.ComponentModel;
using RecipeEditor.Models;

namespace RecipeEditor.ViewModels;

public partial class ParameterViewModel : ObservableObject
{
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private double _value;
    [ObservableProperty] private string _unit = "";
    [ObservableProperty] private double _minValue;
    [ObservableProperty] private double _maxValue;
    [ObservableProperty] private bool _isValid = true;

    public ParameterViewModel(StepParameter model)
    {
        _name = model.Name;
        _value = model.Value;
        _unit = model.Unit;
        _minValue = model.MinValue;
        _maxValue = model.MaxValue;
        _isValid = model.Value >= model.MinValue && model.Value <= model.MaxValue;
    }

    partial void OnValueChanged(double value)
    {
        IsValid = value >= MinValue && value <= MaxValue;
    }

    public string RangeText => $"[{MinValue} ~ {MaxValue}] {Unit}";

    public StepParameter ToModel() => new()
    {
        Name = Name,
        Value = Value,
        Unit = Unit,
        MinValue = MinValue,
        MaxValue = MaxValue
    };
}
