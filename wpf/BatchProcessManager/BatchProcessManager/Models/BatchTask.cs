using CommunityToolkit.Mvvm.ComponentModel;

namespace BatchProcessManager.Models;

public enum BatchTaskStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Retrying,
    Cancelled
}

public enum TaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public partial class BatchTask : ObservableObject
{
    [ObservableProperty] private string _id = Guid.NewGuid().ToString("N")[..8].ToUpper();
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private BatchTaskStatus _status = BatchTaskStatus.Pending;
    [ObservableProperty] private TaskPriority _priority = TaskPriority.Normal;
    [ObservableProperty] private int _retryCount = 0;
    [ObservableProperty] private int _maxRetries = 3;
    [ObservableProperty] private double _progress = 0;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private DateTime _createdAt = DateTime.Now;
    [ObservableProperty] private DateTime? _startedAt;
    [ObservableProperty] private DateTime? _completedAt;
    [ObservableProperty] private int _durationSeconds = 3;

    public bool CanRetry => RetryCount < MaxRetries && Status == BatchTaskStatus.Failed;
    public bool IsFinished => Status == BatchTaskStatus.Completed || Status == BatchTaskStatus.Failed || Status == BatchTaskStatus.Cancelled;

    public string StatusText => Status switch
    {
        BatchTaskStatus.Pending => "等待中",
        BatchTaskStatus.Running => "執行中",
        BatchTaskStatus.Completed => "已完成",
        BatchTaskStatus.Failed => "失敗",
        BatchTaskStatus.Retrying => "重試中",
        BatchTaskStatus.Cancelled => "已取消",
        _ => "未知"
    };

    public string PriorityText => Priority switch
    {
        TaskPriority.Low => "低",
        TaskPriority.Normal => "一般",
        TaskPriority.High => "高",
        TaskPriority.Critical => "緊急",
        _ => "一般"
    };
}
