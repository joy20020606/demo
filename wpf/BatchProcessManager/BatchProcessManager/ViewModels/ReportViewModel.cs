using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BatchProcessManager.Models;
using BatchProcessManager.Services;

namespace BatchProcessManager.ViewModels;

public partial class ReportViewModel : ObservableObject
{
    private readonly ReportService _reportService = new();

    [ObservableProperty] private ObservableCollection<TaskResult> _results = [];
    [ObservableProperty] private string _exportStatus = string.Empty;
    [ObservableProperty] private string _lastExportPath = string.Empty;

    public int TotalCount => Results.Count;
    public int SuccessCount => Results.Count(r => r.IsSuccess);
    public int FailedCount => Results.Count(r => !r.IsSuccess);
    public string SuccessRate => TotalCount > 0 ? $"{SuccessCount * 100.0 / TotalCount:F1}%" : "0%";
    public string TotalDuration =>
        TimeSpan.FromSeconds(Results.Sum(r => r.Duration.TotalSeconds)).ToString(@"mm\:ss\.f");

    public void UpdateResults(IEnumerable<TaskResult> results)
    {
        Results.Clear();
        foreach (var r in results) Results.Add(r);
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(SuccessCount));
        OnPropertyChanged(nameof(FailedCount));
        OnPropertyChanged(nameof(SuccessRate));
        OnPropertyChanged(nameof(TotalDuration));
    }

    [RelayCommand]
    private async Task ExportReport()
    {
        if (!Results.Any())
        {
            ExportStatus = "無資料可匯出";
            return;
        }

        string fileName = $"BatchReport_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

        try
        {
            await _reportService.ExportAsync(Results, path);
            LastExportPath = path;
            ExportStatus = $"已匯出：{fileName}";
        }
        catch (Exception ex)
        {
            ExportStatus = $"匯出失敗：{ex.Message}";
        }
    }
}
