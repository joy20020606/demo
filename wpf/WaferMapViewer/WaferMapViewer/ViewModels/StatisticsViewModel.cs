using CommunityToolkit.Mvvm.ComponentModel;
using WaferMapViewer.Models;

namespace WaferMapViewer.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    [ObservableProperty] private double _yield;
    [ObservableProperty] private int _totalDies;
    [ObservableProperty] private int _passCount;
    [ObservableProperty] private int _failCount;
    [ObservableProperty] private string _waferId = "-";
    [ObservableProperty] private string _lotId = "-";
    [ObservableProperty] private string _inspectionTime = "-";

    private List<DefectStat> _defectStats = new();
    public List<DefectStat> DefectStats
    {
        get => _defectStats;
        private set => SetProperty(ref _defectStats, value);
    }

    public void Update(WaferMap wafer)
    {
        WaferId = wafer.WaferId;
        LotId = wafer.LotId;
        InspectionTime = wafer.InspectionTime.ToString("yyyy-MM-dd HH:mm:ss");
        TotalDies = wafer.TotalDies;
        PassCount = wafer.PassCount;
        FailCount = wafer.FailCount;
        Yield = wafer.Yield;

        var defectTypes = new[]
        {
            InspectionResult.Fail, InspectionResult.Scratch,
            InspectionResult.Particle, InspectionResult.Void, InspectionResult.Crack
        };

        DefectStats = defectTypes
            .Select(r => new DefectStat(
                r.ToString(),
                wafer.Dies.Count(d => d.Result == r),
                TotalDies > 0 ? wafer.Dies.Count(d => d.Result == r) * 100.0 / TotalDies : 0))
            .Where(s => s.Count > 0)
            .ToList();
    }
}

public record DefectStat(string DefectType, int Count, double Percentage);
