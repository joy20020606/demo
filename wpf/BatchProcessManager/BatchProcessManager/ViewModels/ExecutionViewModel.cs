using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BatchProcessManager.Models;
using BatchProcessManager.Services;

namespace BatchProcessManager.ViewModels;

public partial class ExecutionViewModel : ObservableObject
{
    private readonly BatchExecutionService _executionService = new();

    [ObservableProperty] private BatchConfig _config = new();
    [ObservableProperty] private double _overallProgress = 0;
    [ObservableProperty] private bool _isRunning = false;
    [ObservableProperty] private string _statusMessage = "就緒";
    [ObservableProperty] private DateTime? _startTime;
    [ObservableProperty] private DateTime? _estimatedEndTime;
    [ObservableProperty] private ObservableCollection<string> _executionLog = [];
    [ObservableProperty] private ObservableCollection<TaskResult> _results = [];

    public event Action? ExecutionCompleted;

    public ExecutionViewModel()
    {
        _executionService.TaskStarted += OnTaskStarted;
        _executionService.TaskCompleted += OnTaskCompleted;
        _executionService.AllCompleted += OnAllCompleted;
    }

    public async Task StartAsync(IEnumerable<BatchTask> tasks)
    {
        Results.Clear();
        ExecutionLog.Clear();
        OverallProgress = 0;
        IsRunning = true;
        StartTime = DateTime.Now;
        StatusMessage = "執行中...";

        AddLog("批次任務開始執行");
        AddLog($"最大並行數: {Config.MaxConcurrency}，最大重試: {Config.MaxRetries}");

        await _executionService.ExecuteAsync(
            tasks,
            Config,
            progress =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    OverallProgress = progress;
                    UpdateEstimatedEndTime(progress);
                });
            });
    }

    [RelayCommand]
    private void Cancel()
    {
        _executionService.Cancel();
        StatusMessage = "取消中...";
        AddLog("使用者取消執行");
    }

    private void OnTaskStarted(BatchTask task)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
            AddLog($"[開始] {task.Name} (優先: {task.PriorityText})"));
    }

    private void OnTaskCompleted(BatchTask task, TaskResult result)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Results.Add(result);
            string icon = result.IsSuccess ? "✓" : "✗";
            string retry = result.RetryCount > 0 ? $" (重試 {result.RetryCount} 次)" : "";
            AddLog($"[{icon}] {task.Name} - {result.DurationText}{retry}");
        });
    }

    private void OnAllCompleted(IReadOnlyList<TaskResult> results)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            IsRunning = false;
            int success = results.Count(r => r.IsSuccess);
            int failed = results.Count(r => !r.IsSuccess);
            StatusMessage = $"完成：{success} 成功，{failed} 失敗";
            AddLog($"--- 批次完成：{success}/{results.Count} 成功 ---");
            ExecutionCompleted?.Invoke();
        });
    }

    private void UpdateEstimatedEndTime(double progress)
    {
        if (progress <= 0 || StartTime == null) return;
        double elapsed = (DateTime.Now - StartTime.Value).TotalSeconds;
        double total = elapsed / (progress / 100.0);
        EstimatedEndTime = StartTime.Value.AddSeconds(total);
    }

    private void AddLog(string message)
    {
        ExecutionLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
        if (ExecutionLog.Count > 200)
            ExecutionLog.RemoveAt(ExecutionLog.Count - 1);
    }

    public string ElapsedText =>
        StartTime.HasValue
            ? (DateTime.Now - StartTime.Value).ToString(@"mm\:ss")
            : "--:--";

    public string EstimatedEndText =>
        EstimatedEndTime.HasValue
            ? EstimatedEndTime.Value.ToString("HH:mm:ss")
            : "--:--:--";
}
