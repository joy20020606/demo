using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstrumentDashboard.Models;
using InstrumentDashboard.Services;

namespace InstrumentDashboard.ViewModels;

public partial class ParameterPanelViewModel : ObservableObject
{
    private readonly RecipeService _recipeService;
    private readonly string _recipesDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "InstrumentDashboard", "Recipes");

    [ObservableProperty] private double _voltage = 5.0;
    [ObservableProperty] private double _current = 0.1;
    [ObservableProperty] private double _scanSpeed = 100.0;
    [ObservableProperty] private double _samplingRate = 1000.0;
    [ObservableProperty] private double _targetTemperature = 25.0;
    [ObservableProperty] private double _targetPressure = 101.325;
    [ObservableProperty] private double _vacuumLevel = 1e-4;
    [ObservableProperty] private int _scanCount = 10;
    [ObservableProperty] private string _recipeName = "New Recipe";
    [ObservableProperty] private Recipe? _selectedRecipe;

    public ObservableCollection<Recipe> Recipes { get; } = new();

    public ParameterPanelViewModel(RecipeService recipeService)
    {
        _recipeService = recipeService;
        Directory.CreateDirectory(_recipesDir);
        LoadRecipes();
    }

    public InstrumentParameter GetParameters() => new()
    {
        Voltage = Voltage,
        Current = Current,
        ScanSpeed = ScanSpeed,
        SamplingRate = SamplingRate,
        TargetTemperature = TargetTemperature,
        TargetPressure = TargetPressure,
        VacuumLevel = VacuumLevel,
        ScanCount = ScanCount
    };

    public void ApplyParameters(InstrumentParameter p)
    {
        Voltage = p.Voltage;
        Current = p.Current;
        ScanSpeed = p.ScanSpeed;
        SamplingRate = p.SamplingRate;
        TargetTemperature = p.TargetTemperature;
        TargetPressure = p.TargetPressure;
        VacuumLevel = p.VacuumLevel;
        ScanCount = p.ScanCount;
    }

    [RelayCommand]
    private void SaveRecipe()
    {
        var recipe = new Recipe
        {
            Name = RecipeName,
            Description = $"Saved on {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            CreatedAt = DateTime.Now,
            Parameters = GetParameters()
        };

        var filePath = Path.Combine(_recipesDir, $"{RecipeName}.json");
        _recipeService.Save(recipe, filePath);
        LoadRecipes();
    }

    [RelayCommand]
    private void LoadRecipe()
    {
        if (SelectedRecipe is null) return;
        RecipeName = SelectedRecipe.Name;
        ApplyParameters(SelectedRecipe.Parameters);
    }

    private void LoadRecipes()
    {
        Recipes.Clear();
        foreach (var r in _recipeService.LoadAll(_recipesDir))
            Recipes.Add(r);
    }
}
