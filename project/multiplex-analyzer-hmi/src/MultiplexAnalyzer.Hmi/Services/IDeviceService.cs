using MultiplexAnalyzer.Hmi.Models;

namespace MultiplexAnalyzer.Hmi.Services;

public interface IDeviceService
{
    event EventHandler<DeviceSnapshot>? SnapshotChanged;

    IReadOnlyList<ProtocolStep> Protocol { get; }

    double TotalMinutes { get; }

    RunState State { get; }

    double ElapsedMinutes { get; }

    void Start();

    void Pause();

    void Resume();

    void Abort();

    void InjectError();
}
