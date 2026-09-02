using System.Windows.Threading;
using MultiplexAnalyzer.Hmi.Models;

namespace MultiplexAnalyzer.Hmi.Services;

public sealed class FakeDeviceService : IDeviceService
{
    private const double MinutesPerTick = 3;

    private readonly DispatcherTimer timer;

    public FakeDeviceService()
    {
        Protocol =
        [
            new ProtocolStep("Load", 5),
            new ProtocolStep("Incubate", 30),
            new ProtocolStep("Wash", 10),
            new ProtocolStep("Read", 15)
        ];

        TotalMinutes = Protocol.Sum(step => step.DurationMinutes);

        timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += OnTick;
    }

    public event EventHandler<DeviceSnapshot>? SnapshotChanged;

    public IReadOnlyList<ProtocolStep> Protocol { get; }

    public double TotalMinutes { get; }

    public RunState State { get; private set; } = RunState.Idle;

    public double ElapsedMinutes { get; private set; }

    public void Start()
    {
        ElapsedMinutes = 0;
        State = RunState.Running;
        timer.Start();
        Publish();
    }

    public void Pause()
    {
        if (State is not RunState.Running)
        {
            return;
        }

        timer.Stop();
        State = RunState.Paused;
        Publish();
    }

    public void Resume()
    {
        if (State is not RunState.Paused)
        {
            return;
        }

        State = RunState.Running;
        timer.Start();
        Publish();
    }

    public void Abort()
    {
        timer.Stop();
        ElapsedMinutes = 0;
        State = RunState.Idle;
        Publish();
    }

    public void InjectError()
    {
        timer.Stop();
        State = RunState.Error;
        Publish();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        ElapsedMinutes = Math.Min(ElapsedMinutes + MinutesPerTick, TotalMinutes);

        if (ElapsedMinutes >= TotalMinutes)
        {
            timer.Stop();
            State = RunState.Completed;
        }

        Publish();
    }

    private void Publish() => SnapshotChanged?.Invoke(this, new DeviceSnapshot(State, ElapsedMinutes));
}
