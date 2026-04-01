using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BatchProcessManager.Models;

namespace BatchProcessManager.ViewModels;

public partial class TaskQueueViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BatchTask> _tasks = [];
    [ObservableProperty] private BatchTask? _selectedTask;
    [ObservableProperty] private string _newTaskName = string.Empty;
    [ObservableProperty] private TaskPriority _newTaskPriority = TaskPriority.Normal;
    [ObservableProperty] private int _newTaskDuration = 3;
    [ObservableProperty] private string _newTaskDescription = string.Empty;

    public TaskPriority[] Priorities { get; } = [TaskPriority.Low, TaskPriority.Normal, TaskPriority.High, TaskPriority.Critical];

    [RelayCommand]
    private void AddTask()
    {
        if (string.IsNullOrWhiteSpace(NewTaskName)) return;

        Tasks.Add(new BatchTask
        {
            Name = NewTaskName.Trim(),
            Priority = NewTaskPriority,
            DurationSeconds = NewTaskDuration,
            Description = NewTaskDescription.Trim()
        });

        NewTaskName = string.Empty;
        NewTaskDescription = string.Empty;
        NewTaskDuration = 3;
        NewTaskPriority = TaskPriority.Normal;
    }

    [RelayCommand]
    private void RemoveTask(BatchTask? task)
    {
        if (task != null && Tasks.Contains(task))
            Tasks.Remove(task);
    }

    [RelayCommand]
    private void MoveUp(BatchTask? task)
    {
        if (task == null) return;
        int idx = Tasks.IndexOf(task);
        if (idx > 0) Tasks.Move(idx, idx - 1);
    }

    [RelayCommand]
    private void MoveDown(BatchTask? task)
    {
        if (task == null) return;
        int idx = Tasks.IndexOf(task);
        if (idx < Tasks.Count - 1) Tasks.Move(idx, idx + 1);
    }

    [RelayCommand]
    private void SortByPriority()
    {
        var sorted = Tasks.OrderByDescending(t => (int)t.Priority).ToList();
        Tasks.Clear();
        foreach (var t in sorted) Tasks.Add(t);
    }

    [RelayCommand]
    private void AddSampleTasks()
    {
        var samples = new[]
        {
            new BatchTask { Name = "影像檢測 #1", Priority = TaskPriority.High, DurationSeconds = 4, Description = "Wafer AOI 掃描" },
            new BatchTask { Name = "資料備份", Priority = TaskPriority.Normal, DurationSeconds = 3, Description = "系統資料備份" },
            new BatchTask { Name = "報表生成", Priority = TaskPriority.Low, DurationSeconds = 2, Description = "每日報表" },
            new BatchTask { Name = "緊急校正", Priority = TaskPriority.Critical, DurationSeconds = 5, Description = "設備校正程序" },
            new BatchTask { Name = "品質分析", Priority = TaskPriority.High, DurationSeconds = 4, Description = "批次品質分析" },
            new BatchTask { Name = "資料同步", Priority = TaskPriority.Normal, DurationSeconds = 3, Description = "跨系統資料同步" },
        };
        foreach (var s in samples) Tasks.Add(s);
    }

    [RelayCommand]
    private void ClearCompleted()
    {
        var finished = Tasks.Where(t => t.IsFinished).ToList();
        foreach (var t in finished) Tasks.Remove(t);
    }

    public void ResetTasksForNewRun()
    {
        foreach (var t in Tasks)
        {
            t.Status = BatchTaskStatus.Pending;
            t.Progress = 0;
            t.RetryCount = 0;
            t.StartedAt = null;
            t.CompletedAt = null;
        }
    }
}
