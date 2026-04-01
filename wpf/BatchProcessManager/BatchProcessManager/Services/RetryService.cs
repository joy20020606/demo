using BatchProcessManager.Models;

namespace BatchProcessManager.Services;

public class RetryService
{
    public async Task<bool> ExecuteWithRetryAsync(
        BatchTask task,
        Func<BatchTask, CancellationToken, Task<bool>> operation,
        int maxRetries,
        int delaySeconds,
        CancellationToken cancellationToken)
    {
        task.RetryCount = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                bool result = await operation(task, cancellationToken);
                if (result) return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
            }

            if (task.RetryCount >= maxRetries)
                return false;

            task.RetryCount++;
            task.Status = BatchTaskStatus.Retrying;

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
        }
    }
}
