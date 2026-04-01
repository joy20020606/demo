using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RecipeEditor.Models;

namespace RecipeEditor.ViewModels;

public partial class StepViewModel : ObservableObject
{
    [ObservableProperty] private string _name = "New Step";
    [ObservableProperty] private StepType _stepType = StepType.Wait;
    [ObservableProperty] private double _durationSeconds = 60;
    [ObservableProperty] private string _stepTypeColor = "#607D8B";

    public ObservableCollection<ParameterViewModel> Parameters { get; } = new();

    public StepViewModel(RecipeStep model)
    {
        _name = model.Name;
        _stepType = model.StepType;
        _durationSeconds = model.DurationSeconds;
        _stepTypeColor = GetStepTypeColor(model.StepType);
        foreach (var p in model.Parameters)
            Parameters.Add(new ParameterViewModel(p));
    }

    partial void OnStepTypeChanged(StepType value)
    {
        StepTypeColor = GetStepTypeColor(value);
    }

    private static string GetStepTypeColor(StepType t) => t switch
    {
        StepType.Deposition => "#1976D2",
        StepType.Etch       => "#D32F2F",
        StepType.Clean      => "#388E3C",
        StepType.Anneal     => "#F57C00",
        StepType.Transfer   => "#7B1FA2",
        _                   => "#607D8B"
    };

    public string DurationDisplay => $"{DurationSeconds:F0} s";

    public RecipeStep ToModel() => new()
    {
        Name = Name,
        StepType = StepType,
        DurationSeconds = DurationSeconds,
        Parameters = Parameters.Select(p => p.ToModel()).ToList()
    };
}
