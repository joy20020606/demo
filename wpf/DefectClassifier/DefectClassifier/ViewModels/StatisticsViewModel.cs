using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DefectClassifier.Models;
using DefectClassifier.Services;

namespace DefectClassifier.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly DefectRepository _repository;
    private readonly StatisticsService _statisticsService;

    public event Action<string>? StatusChanged;

    [ObservableProperty]
    private int _totalDefects;

    [ObservableProperty]
    private double _yield;

    [ObservableProperty]
    private int _waferCount;

    public ObservableCollection<ChartItem> TypeChartData { get; } = new();
    public ObservableCollection<ChartItem> TrendChartData { get; } = new();
    public ObservableCollection<ChartItem> WaferChartData { get; } = new();

    private static readonly Dictionary<DefectType, Brush> TypeBrushes = new()
    {
        [DefectType.Scratch]      = new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)),
        [DefectType.Particle]     = new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12)),
        [DefectType.PatternDefect]= new SolidColorBrush(Color.FromRgb(0x9B, 0x59, 0xB6)),
        [DefectType.Pit]          = new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
        [DefectType.Bridge]       = new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x71)),
        [DefectType.Unknown]      = new SolidColorBrush(Color.FromRgb(0x95, 0xA5, 0xA6)),
    };

    public StatisticsViewModel(DefectRepository repository, StatisticsService statisticsService)
    {
        _repository = repository;
        _statisticsService = statisticsService;
    }

    public void Refresh()
    {
        var records = _repository.GetAll();
        TotalDefects = records.Count;
        Yield = _statisticsService.CalculateYield(records);

        var dist = _statisticsService.GetTypeDistribution(records);
        var maxDist = dist.Values.DefaultIfEmpty(1).Max();
        TypeChartData.Clear();
        foreach (var kvp in dist.OrderByDescending(k => k.Value))
        {
            TypeChartData.Add(new ChartItem
            {
                Label = BrowserViewModel.GetTypeName(kvp.Key),
                Value = kvp.Value,
                BarBrush = TypeBrushes.GetValueOrDefault(kvp.Key, Brushes.SteelBlue),
                BarHeight = maxDist > 0 ? kvp.Value / (double)maxDist * 160 : 0
            });
        }

        var trend = _statisticsService.GetDailyTrend(records);
        var maxTrend = trend.Values.DefaultIfEmpty(1).Max();
        TrendChartData.Clear();
        foreach (var kvp in trend)
        {
            TrendChartData.Add(new ChartItem
            {
                Label = kvp.Key.ToString("MM/dd"),
                Value = kvp.Value,
                BarBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC)),
                BarHeight = maxTrend > 0 ? kvp.Value / (double)maxTrend * 120 : 0
            });
        }

        var wafer = _statisticsService.GetWaferDefectCounts(records);
        WaferCount = wafer.Count;
        var maxWafer = wafer.Values.DefaultIfEmpty(1).Max();
        WaferChartData.Clear();
        foreach (var kvp in wafer)
        {
            WaferChartData.Add(new ChartItem
            {
                Label = kvp.Key,
                Value = kvp.Value,
                BarBrush = new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)),
                BarHeight = maxWafer > 0 ? kvp.Value / (double)maxWafer * 120 : 0
            });
        }

        StatusChanged?.Invoke($"統計已更新，共 {TotalDefects} 筆缺陷，良率 {Yield}%");
    }

    [RelayCommand]
    private void RefreshStats() => Refresh();
}
