using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WaferMapViewer.Models;
using WaferMapViewer.Services;

namespace WaferMapViewer.ViewModels;

public partial class WaferMapViewModel : ObservableObject
{
    [ObservableProperty] private WaferMap? _waferMap;
    [ObservableProperty] private DieInfo? _selectedDie;
    [ObservableProperty] private string _selectedColorMap = "Standard";
    [ObservableProperty] private InspectionResult? _highlightResult;
    [ObservableProperty] private bool _isHighlightActive;

    public IReadOnlyList<string> ColorMapNames => ColorMapFactory.AvailableNames;

    public ObservableCollection<FilterItem> FilterItems { get; } = new()
    {
        new FilterItem(null, "All"),
        new FilterItem(InspectionResult.Pass, "Pass"),
        new FilterItem(InspectionResult.Fail, "Fail"),
        new FilterItem(InspectionResult.Scratch, "Scratch"),
        new FilterItem(InspectionResult.Particle, "Particle"),
        new FilterItem(InspectionResult.Void, "Void"),
        new FilterItem(InspectionResult.Crack, "Crack"),
    };

    private FilterItem? _selectedFilter;
    public FilterItem? SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            SetProperty(ref _selectedFilter, value);
            HighlightResult = value?.Result;
            IsHighlightActive = value?.Result != null;
        }
    }

    public void LoadWafer(WaferMap wafer)
    {
        WaferMap = wafer;
        SelectedDie = null;
        SelectedFilter = FilterItems[0];
    }
}

public record FilterItem(InspectionResult? Result, string Label);
