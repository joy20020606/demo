using SocketCommSimulator.Models;

namespace SocketCommSimulator.Services;

public class HeartbeatService
{
    private CancellationTokenSource? _cts;
    private DateTime _lastHeartbeat = DateTime.MinValue;
    private readonly int _intervalMs;
    private readonly int _timeoutMs;

    public event Action? OnHeartbeatSend;
    public event Action? OnTimeout;
    public event Action<LogEntry>? OnLog;

    public bool IsRunning { get; private set; }

    public HeartbeatService(int intervalMs = 5000, int timeoutMs = 15000)
    {
        _intervalMs = intervalMs;
        _timeoutMs = timeoutMs;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        IsRunning = true;
        _lastHeartbeat = DateTime.Now;
        _ = RunAsync(_cts.Token);
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(_intervalMs, ct);

                OnHeartbeatSend?.Invoke();
                OnLog?.Invoke(LogEntry.CreateSystem("Heartbeat 發送"));

                var elapsed = (DateTime.Now - _lastHeartbeat).TotalMilliseconds;
                if (elapsed > _timeoutMs)
                {
                    OnLog?.Invoke(LogEntry.CreateSystem($"Heartbeat 超時 ({elapsed:0}ms)"));
                    OnTimeout?.Invoke();
                }
            }
        }
        catch (OperationCanceledException) { }

        IsRunning = false;
    }

    public void RecordHeartbeatReceived()
    {
        _lastHeartbeat = DateTime.Now;
    }

    public void Stop()
    {
        _cts?.Cancel();
        IsRunning = false;
    }
}
