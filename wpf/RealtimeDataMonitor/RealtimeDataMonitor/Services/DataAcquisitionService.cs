using System.Collections.Concurrent;
using RealtimeDataMonitor.Models;

namespace RealtimeDataMonitor.Services;

public class DataAcquisitionService : IDisposable
{
    private readonly ConcurrentQueue<SensorData> _dataQueue = new();
    private readonly List<ChannelConfig> _channels;
    private readonly Random _random = new();
    private readonly double[] _phases;
    private readonly double[] _frequencies;

    private CancellationTokenSource? _cts;
    private Task? _producerTask;
    private bool _disposed;

    public int SamplesPerSecond { get; set; } = 200;

    public event Action<IReadOnlyList<SensorData>>? DataAvailable;

    public DataAcquisitionService(List<ChannelConfig> channels)
    {
        _channels = channels;
        _phases = new double[channels.Count];
        _frequencies = channels.Select((_, i) => 0.5 + i * 0.3).ToArray();
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _producerTask = Task.Run(() => ProduceDataAsync(_cts.Token));
        _ = Task.Run(() => ConsumeDataAsync(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private async Task ProduceDataAsync(CancellationToken token)
    {
        var intervalMs = 1000.0 / SamplesPerSecond;
        var dt = 1.0 / SamplesPerSecond;

        while (!token.IsCancellationRequested)
        {
            var now = DateTime.Now;

            foreach (var ch in _channels)
            {
                if (!ch.IsEnabled) continue;

                var idx = ch.Id;
                _phases[idx] += 2 * Math.PI * _frequencies[idx] * dt;

                var signal = 50
                    + 30 * Math.Sin(_phases[idx])
                    + 10 * Math.Sin(_phases[idx] * 3.1)
                    + (_random.NextDouble() - 0.5) * 8;

                if (_random.NextDouble() < 0.005)
                    signal += (_random.NextDouble() > 0.5 ? 1 : -1) * 35;

                _dataQueue.Enqueue(new SensorData(ch.Id, signal, now));
            }

            await Task.Delay(TimeSpan.FromMilliseconds(intervalMs), token).ConfigureAwait(false);
        }
    }

    private async Task ConsumeDataAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var batch = new List<SensorData>(64);

            while (_dataQueue.TryDequeue(out var item))
                batch.Add(item);

            if (batch.Count > 0)
                DataAvailable?.Invoke(batch);

            await Task.Delay(50, token).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
