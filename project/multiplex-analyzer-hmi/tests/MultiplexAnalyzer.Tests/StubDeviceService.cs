using MultiplexAnalyzer.Hmi.Models;
using MultiplexAnalyzer.Hmi.Services;

namespace MultiplexAnalyzer.Tests;

public sealed class StubDeviceService : IDeviceService
{
    public event EventHandler<DeviceSnapshot>? SnapshotChanged;

    public IReadOnlyList<ProtocolStep> Protocol { get; } =
    [
        new ProtocolStep("Load", 5),
        new ProtocolStep("Incubate", 30),
        new ProtocolStep("Wash", 10),
        new ProtocolStep("Read", 15)
    ];

    public double TotalMinutes => 60;

    public RunState State { get; private set; } = RunState.Idle;

    public double ElapsedMinutes { get; private set; }

    public List<string> Calls { get; } = [];

    public void Start()
    {
        Calls.Add(nameof(Start));
        Emit(RunState.Running, 0);
    }

    public void Pause()
    {
        Calls.Add(nameof(Pause));
        Emit(RunState.Paused, ElapsedMinutes);
    }

    public void Resume()
    {
        Calls.Add(nameof(Resume));
        Emit(RunState.Running, ElapsedMinutes);
    }

    public void Abort()
    {
        Calls.Add(nameof(Abort));
        Emit(RunState.Idle, 0);
    }

    public void InjectError()
    {
        Calls.Add(nameof(InjectError));
        Emit(RunState.Error, ElapsedMinutes);
    }

    public void Emit(RunState state, double elapsedMinutes)
    {
        State = state;
        ElapsedMinutes = elapsedMinutes;
        SnapshotChanged?.Invoke(this, new DeviceSnapshot(state, elapsedMinutes));
    }
}
