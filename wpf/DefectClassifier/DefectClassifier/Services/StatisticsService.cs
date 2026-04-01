using DefectClassifier.Models;

namespace DefectClassifier.Services;

public class StatisticsService
{
    public Dictionary<DefectType, int> GetTypeDistribution(IReadOnlyList<DefectRecord> records)
    {
        return records
            .GroupBy(r => r.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public Dictionary<string, int> GetWaferDefectCounts(IReadOnlyList<DefectRecord> records)
    {
        return records
            .GroupBy(r => r.WaferId)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public double CalculateYield(IReadOnlyList<DefectRecord> records, int totalDies = 100)
    {
        var criticalCount = records.Count(r => r.Severity >= 3);
        var yield = Math.Max(0.0, (totalDies - criticalCount) / (double)totalDies * 100.0);
        return Math.Round(yield, 2);
    }

    public Dictionary<DateTime, int> GetDailyTrend(IReadOnlyList<DefectRecord> records)
    {
        return records
            .GroupBy(r => r.Timestamp.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
