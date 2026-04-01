using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeEditor.Models;

namespace RecipeEditor.ViewModels;

public partial class RecipeViewModel : ObservableObject
{
    private readonly UndoRedoManager _undoRedo;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    [ObservableProperty] private string _name = "New Recipe";
    [ObservableProperty] private string _version = "1.0.0";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private string _author = "";
    [ObservableProperty] private StepViewModel? _selectedStep;

    public ObservableCollection<StepViewModel> Steps { get; } = new();

    public RecipeViewModel(UndoRedoManager undoRedo)
    {
        _undoRedo = undoRedo;
    }

    [RelayCommand]
    private void AddStep(StepType stepType)
    {
        SaveMemento();
        var step = CreateDefaultStep(stepType);
        var vm = new StepViewModel(step);
        Steps.Add(vm);
        SelectedStep = vm;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedStep))]
    private void RemoveStep()
    {
        if (SelectedStep == null) return;
        SaveMemento();
        var idx = Steps.IndexOf(SelectedStep);
        Steps.Remove(SelectedStep);
        SelectedStep = Steps.Count > 0 ? Steps[Math.Max(0, idx - 1)] : null;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedStep))]
    private void DuplicateStep()
    {
        if (SelectedStep == null) return;
        SaveMemento();
        var cloned = SelectedStep.ToModel().Clone();
        cloned.Id = Guid.NewGuid().ToString();
        cloned.Name += " (Copy)";
        var vm = new StepViewModel(cloned);
        var idx = Steps.IndexOf(SelectedStep);
        Steps.Insert(idx + 1, vm);
        SelectedStep = vm;
    }

    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveStepUp()
    {
        if (SelectedStep == null) return;
        var idx = Steps.IndexOf(SelectedStep);
        if (idx <= 0) return;
        SaveMemento();
        Steps.Move(idx, idx - 1);
    }

    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveStepDown()
    {
        if (SelectedStep == null) return;
        var idx = Steps.IndexOf(SelectedStep);
        if (idx >= Steps.Count - 1) return;
        SaveMemento();
        Steps.Move(idx, idx + 1);
    }

    public void MoveStep(StepViewModel source, StepViewModel target)
    {
        var srcIdx = Steps.IndexOf(source);
        var tgtIdx = Steps.IndexOf(target);
        if (srcIdx < 0 || tgtIdx < 0 || srcIdx == tgtIdx) return;
        SaveMemento();
        Steps.Move(srcIdx, tgtIdx);
        SelectedStep = source;
    }

    private bool HasSelectedStep() => SelectedStep != null;
    private bool CanMoveUp() => SelectedStep != null && Steps.IndexOf(SelectedStep) > 0;
    private bool CanMoveDown() => SelectedStep != null && Steps.IndexOf(SelectedStep) < Steps.Count - 1;

    partial void OnSelectedStepChanged(StepViewModel? value)
    {
        RemoveStepCommand.NotifyCanExecuteChanged();
        DuplicateStepCommand.NotifyCanExecuteChanged();
        MoveStepUpCommand.NotifyCanExecuteChanged();
        MoveStepDownCommand.NotifyCanExecuteChanged();
    }

    private void SaveMemento() => _undoRedo.SaveState(ToJson());

    public string ToJson() => JsonSerializer.Serialize(ToModel(), _jsonOptions);

    public void FromJson(string json)
    {
        var model = JsonSerializer.Deserialize<Recipe>(json, _jsonOptions);
        if (model != null) LoadFromModel(model);
    }

    public Recipe ToModel() => new()
    {
        Name = Name,
        Version = Version,
        Description = Description,
        Author = Author,
        Steps = Steps.Select(s => s.ToModel()).ToList()
    };

    public void LoadFromModel(Recipe model)
    {
        Name = model.Name;
        Version = model.Version;
        Description = model.Description;
        Author = model.Author;
        Steps.Clear();
        foreach (var step in model.Steps)
            Steps.Add(new StepViewModel(step));
        SelectedStep = Steps.FirstOrDefault();
    }

    public void NewRecipe()
    {
        LoadFromModel(new Recipe());
    }

    private static RecipeStep CreateDefaultStep(StepType stepType) => new()
    {
        Name = $"{stepType} Step",
        StepType = stepType,
        DurationSeconds = stepType switch
        {
            StepType.Clean      => 120,
            StepType.Deposition => 300,
            StepType.Etch       => 180,
            StepType.Anneal     => 600,
            StepType.Transfer   => 30,
            _                   => 60
        },
        Parameters = GetDefaultParameters(stepType)
    };

    private static List<StepParameter> GetDefaultParameters(StepType stepType) =>
        stepType switch
        {
            StepType.Deposition => new()
            {
                new() { Name = "Temperature", Value = 350, Unit = "°C",   MinValue = 200, MaxValue = 600 },
                new() { Name = "Pressure",    Value = 0.5, Unit = "mTorr",MinValue = 0.1, MaxValue = 10  },
                new() { Name = "SiH4 Flow",   Value = 100, Unit = "sccm", MinValue = 0,   MaxValue = 500 },
                new() { Name = "N2O Flow",    Value = 200, Unit = "sccm", MinValue = 0,   MaxValue = 1000}
            },
            StepType.Etch => new()
            {
                new() { Name = "Temperature", Value = 25,  Unit = "°C",   MinValue = 20,  MaxValue = 100 },
                new() { Name = "Pressure",    Value = 5,   Unit = "mTorr",MinValue = 1,   MaxValue = 50  },
                new() { Name = "CF4 Flow",    Value = 50,  Unit = "sccm", MinValue = 0,   MaxValue = 200 },
                new() { Name = "O2 Flow",     Value = 10,  Unit = "sccm", MinValue = 0,   MaxValue = 100 },
                new() { Name = "RF Power",    Value = 100, Unit = "W",    MinValue = 0,   MaxValue = 500 }
            },
            StepType.Clean => new()
            {
                new() { Name = "Temperature",  Value = 80, Unit = "°C",   MinValue = 20,  MaxValue = 150 },
                new() { Name = "H2O2 Conc.",   Value = 30, Unit = "%",    MinValue = 0,   MaxValue = 100 },
                new() { Name = "NH3 Conc.",    Value = 5,  Unit = "%",    MinValue = 0,   MaxValue = 30  }
            },
            StepType.Anneal => new()
            {
                new() { Name = "Temperature", Value = 900, Unit = "°C",      MinValue = 400, MaxValue = 1200 },
                new() { Name = "N2 Flow",     Value = 500, Unit = "sccm",    MinValue = 0,   MaxValue = 2000 },
                new() { Name = "Ramp Rate",   Value = 10,  Unit = "°C/min",  MinValue = 1,   MaxValue = 50  }
            },
            StepType.Transfer => new()
            {
                new() { Name = "Speed",        Value = 100, Unit = "mm/s",  MinValue = 1,  MaxValue = 500 },
                new() { Name = "Acceleration", Value = 50,  Unit = "mm/s2", MinValue = 1,  MaxValue = 200 }
            },
            _ => new()
            {
                new() { Name = "Duration", Value = 60, Unit = "s", MinValue = 1, MaxValue = 3600 }
            }
        };
}
