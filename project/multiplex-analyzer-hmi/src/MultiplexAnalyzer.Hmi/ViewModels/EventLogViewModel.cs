using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiplexAnalyzer.Hmi.Models;
using MultiplexAnalyzer.Hmi.Services;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public sealed partial class EventLogViewModel : PageViewModelBase
{
    private const int EntryCount = 5000;

    private readonly ObservableCollection<LogEntry> entries;

    [ObservableProperty]
    private string selectedLevel = "All";

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isVirtualizationEnabled = true;

    [ObservableProperty]
    private int visibleCount;

    [ObservableProperty]
    private double lastRefreshMs;

    [ObservableProperty]
    private int realizedContainers;

    [ObservableProperty]
    private double workingSetMb;

    public EventLogViewModel(ILogService logService)
    {
        entries = new ObservableCollection<LogEntry>(logService.Load(EntryCount));
        Entries = CollectionViewSource.GetDefaultView(entries);
        Entries.Filter = Matches;
        VisibleCount = entries.Count;
    }

    public event EventHandler? MeasurementRequested;

    public override string Title => "Event Log";

    public override string Subtitle => "5,000 entries. Toggle virtualization and watch the realized container count.";

    public string[] Levels { get; } = ["All", "Info", "Warning", "Error"];

    public ICollectionView Entries { get; }

    public int TotalCount => entries.Count;

    public void ReportMeasurement(double refreshMs, int realizedContainers, double workingSetMb)
    {
        LastRefreshMs = Math.Round(refreshMs, 1);
        RealizedContainers = realizedContainers;
        WorkingSetMb = Math.Round(workingSetMb, 1);
    }

    public void RequestMeasurement() => MeasurementRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedLevel = "All";
        SearchText = string.Empty;
    }

    partial void OnSelectedLevelChanged(string value) => ApplyFilter();

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnIsVirtualizationEnabledChanged(bool value) => RequestMeasurement();

    private void ApplyFilter()
    {
        Entries.Refresh();
        VisibleCount = Entries is CollectionView view ? view.Count : entries.Count;
        RequestMeasurement();
    }

    private bool Matches(object item)
    {
        if (item is not LogEntry entry)
        {
            return false;
        }

        var levelMatches = SelectedLevel == "All" || entry.Level.ToString() == SelectedLevel;
        var textMatches = string.IsNullOrWhiteSpace(SearchText)
            || entry.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || entry.Source.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

        return levelMatches && textMatches;
    }
}
