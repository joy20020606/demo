using System.IO;
using System.Text.Json;
using BatchProcessManager.Models;

namespace BatchProcessManager.Services;

public class ReportService
{
    public async Task<string> ExportAsync(IEnumerable<TaskResult> results, string outputPath)
    {
        var report = new
        {
            GeneratedAt = DateTime.Now,
            Summary = new
            {
                Total = results.Count(),
                Succeeded = results.Count(r => r.IsSuccess),
                Failed = results.Count(r => !r.IsSuccess),
                TotalDuration = TimeSpan.FromSeconds(results.Sum(r => r.Duration.TotalSeconds)).ToString(@"mm\:ss\.f"),
                SuccessRate = results.Any() ? $"{results.Count(r => r.IsSuccess) * 100.0 / results.Count():F1}%" : "0%"
            },
            Results = results.Select(r => new
            {
                r.TaskId,
                r.TaskName,
                r.StatusText,
                r.DurationText,
                r.RetryCount,
                CompletedAt = r.CompletedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                r.ErrorMessage
            })
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(report, options);
        await File.WriteAllTextAsync(outputPath, json);
        return outputPath;
    }
}
