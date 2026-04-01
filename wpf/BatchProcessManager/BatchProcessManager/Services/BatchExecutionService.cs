using System.Collections.Concurrent;
using BatchProcessManager.Models;

namespace BatchProcessManager.Services;

public class BatchExecutionService
{
    private readonly RetryService _retryService = new();
    private CancellationTokenSource? _cts;

    public event Action<BatchTask>? TaskStarted;
    public event Action<BatchTask, TaskResult>? TaskCompleted;
    public event Action<IReadOnlyList<TaskResult>>? AllCompleted;

    public bool IsRunning { get; private set; }

    public async Task ExecuteAsync(
        IEnumerable<BatchTask> tasks,
        BatchConfig config,
        Action<double>? progressCallback = null)
    {
        _cts = new CancellationTokenSource();
        IsRunning = true;

        var taskList = tasks
            .Where(t => t.Status == BatchTaskStatus.Pending)
            .OrderByDescending(t => (int)t.Priority)
            .ToList();

        var results = new ConcurrentBag<TaskResult>();
        int completed = 0;
        int total = taskList.Count;

        using var semaphore = new SemaphoreSlim(config.MaxConcurrency, config.MaxConcurrency);

        var taskRunners = taskList.Select(async batchTask =>
        {
            await semaphore.WaitAsync(_cts.Token);
            try
            {
                _cts.Token.ThrowIfCancellationRequested();

                batchTask.Status = BatchTaskStatus.Running;
                batchTask.StartedAt = DateTime.Now;
                batchTask.Progress = 0;
                TaskStarted?.Invoke(batchTask);

                var startTime = DateTime.Now;
                bool success = false;

                success = await _retryService.ExecuteWithRetryAsync(
                    batchTask,
                    SimulateTaskAsync,
                    config.MaxRetries,
                    config.RetryDelaySeconds,
                    _cts.Token);

                var duration = DateTime.Now - startTime;
                batchTask.CompletedAt = DateTime.Now;
                batchTask.Status = success ? BatchTaskStatus.Completed : BatchTaskStatus.Failed;
                batchTask.Progress = success ? 100 : batchTask.Progress;

                var result = new TaskResult
                {
                    TaskId = batchTask.Id,
                    TaskName = batchTask.Name,
                    IsSuccess = success,
                    Duration = duration,
                    ErrorMessage = success ? null : "任務執行失敗",
                    RetryCount = batchTask.RetryCount,
                    CompletedAt = DateTime.Now
                };

                results.Add(result);
                TaskCompleted?.Invoke(batchTask, result);

                var c = Interlocked.Increment(ref completed);
                progressCallback?.Invoke((double)c / total * 100);

                if (config.StopOnFirstFailure && !success)
                    _cts.Cancel();
            }
            catch (OperationCanceledException)
            {
                batchTask.Status = BatchTaskStatus.Cancelled;
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(taskRunners);

        IsRunning = false;
        AllCompleted?.Invoke(results.ToList());
    }

    private static readonly Random _random = new();

    private async Task<bool> SimulateTaskAsync(BatchTask task, CancellationToken ct)
    {
        int steps = 10;
        int stepMs = (task.DurationSeconds * 1000) / steps;

        for (int i = 0; i < steps; i++)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(stepMs, ct);
            task.Progress = (i + 1) * 10.0;
        }

        double failChance = task.Priority == TaskPriority.Critical ? 0.05 : 0.15;
        return _random.NextDouble() > failChance;
    }

    public void Cancel()
    {
        _cts?.Cancel();
    }
}
