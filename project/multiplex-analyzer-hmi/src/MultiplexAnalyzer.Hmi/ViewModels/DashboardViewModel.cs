using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiplexAnalyzer.Hmi.Models;
using MultiplexAnalyzer.Hmi.Services;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public sealed partial class DashboardViewModel : PageViewModelBase
{
    private readonly IDeviceService device;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseResumeCommand))]
    [NotifyCanExecuteChangedFor(nameof(RequestAbortCommand))]
    [NotifyCanExecuteChangedFor(nameof(InjectErrorCommand))]
    [NotifyPropertyChangedFor(nameof(PauseResumeLabel))]
    private RunState state = RunState.Idle;

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private string progressLabel = "0%";

    [ObservableProperty]
    private string remainingLabel = "--:--";

    [ObservableProperty]
    private bool isAbortDialogOpen;

    public DashboardViewModel(IDeviceService device)
    {
        this.device = device;
        Steps = new ObservableCollection<ProtocolStepViewModel>(
            device.Protocol.Select(step => new ProtocolStepViewModel(step.Name, step.DurationMinutes)));

        device.SnapshotChanged += OnSnapshotChanged;
    }

    public override string Title => "Run Dashboard";

    public override string Subtitle => "Protocol state machine, hand-drawn progress ring, modal confirmation";

    public ObservableCollection<ProtocolStepViewModel> Steps { get; }

    public string PauseResumeLabel => State is RunState.Paused ? "Resume" : "Pause";

    [RelayCommand(CanExecute = nameof(CanStart))]

    private void Start() => device.Start();

    private bool CanStart() => State is RunState.Idle or RunState.Completed or RunState.Error;

    [RelayCommand(CanExecute = nameof(CanPauseResume))]
    private void PauseResume()
    {
        if (State is RunState.Paused)
        {
            device.Resume();
        }
        else
        {
            device.Pause();
        }
    }

    private bool CanPauseResume() => State is RunState.Running or RunState.Paused;

    [RelayCommand(CanExecute = nameof(CanAbort))]
    private void RequestAbort() => IsAbortDialogOpen = true;

    private bool CanAbort() => State is RunState.Running or RunState.Paused;

    [RelayCommand]
    private void ConfirmAbort()
    {
        IsAbortDialogOpen = false;
        device.Abort();
    }

    [RelayCommand]
    private void CancelAbort() => IsAbortDialogOpen = false;

    [RelayCommand(CanExecute = nameof(CanInjectError))]
    private void InjectError() => device.InjectError();

    private bool CanInjectError() => State is RunState.Running or RunState.Paused;

    private void OnSnapshotChanged(object? sender, DeviceSnapshot snapshot)
    {
        State = snapshot.State;
        Progress = snapshot.ElapsedMinutes / device.TotalMinutes;
        ProgressLabel = $"{Progress * 100:0}%";

        var remaining = TimeSpan.FromMinutes(device.TotalMinutes - snapshot.ElapsedMinutes);
        RemainingLabel = State is RunState.Idle ? "--:--" : $"{remaining:hh\\:mm}";

        UpdateSteps(snapshot.ElapsedMinutes);
    }

    private void UpdateSteps(double elapsedMinutes)
    {
        var boundary = 0d;

        foreach (var step in Steps)
        {
            var start = boundary;
            boundary += step.DurationMinutes;

            step.IsDone = State is not RunState.Idle && elapsedMinutes >= boundary;
            step.IsActive = State is RunState.Running or RunState.Paused
                && elapsedMinutes >= start
                && elapsedMinutes < boundary;
        }
    }
}
