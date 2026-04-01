using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeEditor.Services;

namespace RecipeEditor.ViewModels;

public class ValidationResultViewModel
{
    public string Message { get; }
    public ValidationSeverity Severity { get; }
    public string Icon => Severity switch
    {
        ValidationSeverity.Error   => "✖",
        ValidationSeverity.Warning => "⚠",
        _                          => "✔"
    };
    public string Color => Severity switch
    {
        ValidationSeverity.Error   => "#D32F2F",
        ValidationSeverity.Warning => "#F57C00",
        _                          => "#388E3C"
    };

    public ValidationResultViewModel(ValidationResult result)
    {
        Message = result.Message;
        Severity = result.Severity;
    }
}

public partial class MainViewModel : ObservableObject
{
    private readonly RecipeService _recipeService = new();
    private readonly ValidationService _validationService = new();

    public UndoRedoManager UndoRedo { get; } = new();
    public RecipeViewModel Recipe { get; }

    [ObservableProperty] private ObservableCollection<ValidationResultViewModel> _validationResults = new();
    [ObservableProperty] private bool _hasValidationErrors;
    [ObservableProperty] private string _statusMessage = "Ready";

    public MainViewModel()
    {
        Recipe = new RecipeViewModel(UndoRedo);
        Recipe.NewRecipe();

        UndoRedo.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(UndoRedoManager.CanUndo))
                UndoCommand.NotifyCanExecuteChanged();
            else if (e.PropertyName == nameof(UndoRedoManager.CanRedo))
                RedoCommand.NotifyCanExecuteChanged();
        };
    }

    [RelayCommand]
    private void NewRecipe()
    {
        Recipe.NewRecipe();
        UndoRedo.Clear();
        ValidationResults.Clear();
        HasValidationErrors = false;
        StatusMessage = "New recipe created.";
    }

    [RelayCommand]
    private async Task OpenRecipe()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Open Recipe"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var model = await _recipeService.LoadAsync(dlg.FileName);
            Recipe.LoadFromModel(model);
            UndoRedo.Clear();
            ValidationResults.Clear();
            HasValidationErrors = false;
            StatusMessage = $"Opened: {System.IO.Path.GetFileName(dlg.FileName)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening file: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveRecipe()
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            DefaultExt = ".json",
            FileName = Recipe.Name,
            Title = "Save Recipe"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var model = Recipe.ToModel();
            await _recipeService.SaveAsync(model, dlg.FileName);
            StatusMessage = $"Saved: {System.IO.Path.GetFileName(dlg.FileName)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        var current = Recipe.ToJson();
        var snapshot = UndoRedo.Undo(current);
        if (snapshot == null) return;
        Recipe.FromJson(snapshot);
        StatusMessage = "Undo.";
    }

    private bool CanUndo() => UndoRedo.CanUndo;

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        var current = Recipe.ToJson();
        var snapshot = UndoRedo.Redo(current);
        if (snapshot == null) return;
        Recipe.FromJson(snapshot);
        StatusMessage = "Redo.";
    }

    private bool CanRedo() => UndoRedo.CanRedo;

    [RelayCommand]
    private void Validate()
    {
        var model = Recipe.ToModel();
        var results = _validationService.Validate(model);
        ValidationResults = new ObservableCollection<ValidationResultViewModel>(
            results.Select(r => new ValidationResultViewModel(r)));
        HasValidationErrors = results.Any(r => r.Severity == ValidationSeverity.Error);
        StatusMessage = HasValidationErrors
            ? $"Validation failed ({results.Count(r => r.Severity == ValidationSeverity.Error)} error(s))."
            : "Validation passed.";
    }
}
