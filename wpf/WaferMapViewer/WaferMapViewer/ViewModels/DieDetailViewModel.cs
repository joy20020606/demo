using CommunityToolkit.Mvvm.ComponentModel;
using WaferMapViewer.Models;

namespace WaferMapViewer.ViewModels;

public partial class DieDetailViewModel : ObservableObject
{
    [ObservableProperty] private DieInfo? _selectedDie;
    [ObservableProperty] private bool _hasSelection;

    partial void OnSelectedDieChanged(DieInfo? value)
    {
        HasSelection = value != null;
    }

    public string RowCol => SelectedDie != null ? $"({SelectedDie.Row}, {SelectedDie.Col})" : "-";
    public string Result => SelectedDie?.Result.ToString() ?? "-";
    public string DefectDensity => SelectedDie != null ? $"{SelectedDie.DefectDensity:F3} def/cm²" : "-";
    public string Description => SelectedDie?.DefectDescription ?? "N/A";
    public string IsEdge => SelectedDie != null ? (SelectedDie.IsEdgeDie ? "Yes" : "No") : "-";

    public List<MeasurementItem> Measurements => SelectedDie?.Measurements
        .Select(kv => new MeasurementItem(kv.Key, $"{kv.Value:F3}"))
        .ToList() ?? new();

    public void Select(DieInfo die)
    {
        SelectedDie = die;
        OnPropertyChanged(nameof(RowCol));
        OnPropertyChanged(nameof(Result));
        OnPropertyChanged(nameof(DefectDensity));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(IsEdge));
        OnPropertyChanged(nameof(Measurements));
    }
}

public record MeasurementItem(string Key, string Value);
