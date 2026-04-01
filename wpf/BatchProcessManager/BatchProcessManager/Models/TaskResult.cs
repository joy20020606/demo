namespace BatchProcessManager.Models;

public class TaskResult
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.Now;

    public string StatusText => IsSuccess ? "成功" : "失敗";
    public string DurationText => $"{Duration.TotalSeconds:F1}s";
}
