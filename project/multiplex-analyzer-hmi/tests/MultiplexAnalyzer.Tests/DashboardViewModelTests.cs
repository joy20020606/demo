using MultiplexAnalyzer.Hmi.Models;
using MultiplexAnalyzer.Hmi.ViewModels;

namespace MultiplexAnalyzer.Tests;

public sealed class DashboardViewModelTests
{
    private readonly StubDeviceService device = new();
    private readonly DashboardViewModel vm;

    public DashboardViewModelTests()
    {
        vm = new DashboardViewModel(device);
    }

    [Fact]
    public void Idle_only_start_is_enabled()
    {
        Assert.True(vm.StartCommand.CanExecute(null));
        Assert.False(vm.PauseResumeCommand.CanExecute(null));
        Assert.False(vm.RequestAbortCommand.CanExecute(null));
        Assert.False(vm.InjectErrorCommand.CanExecute(null));
    }

    [Fact]
    public void Start_transitions_to_running_and_flips_command_availability()
    {
        vm.StartCommand.Execute(null);

        Assert.Equal(RunState.Running, vm.State);
        Assert.Contains("Start", device.Calls);
        Assert.False(vm.StartCommand.CanExecute(null));
        Assert.True(vm.PauseResumeCommand.CanExecute(null));
        Assert.True(vm.RequestAbortCommand.CanExecute(null));
    }

    [Fact]
    public void Pause_resume_toggles_label_and_delegates_to_device()
    {
        vm.StartCommand.Execute(null);

        vm.PauseResumeCommand.Execute(null);
        Assert.Equal(RunState.Paused, vm.State);
        Assert.Equal("Resume", vm.PauseResumeLabel);

        vm.PauseResumeCommand.Execute(null);
        Assert.Equal(RunState.Running, vm.State);
        Assert.Equal("Pause", vm.PauseResumeLabel);
        Assert.Equal(["Start", "Pause", "Resume"], device.Calls);
    }

    [Fact]
    public void Abort_requires_confirmation_before_touching_device()
    {
        vm.StartCommand.Execute(null);

        vm.RequestAbortCommand.Execute(null);
        Assert.True(vm.IsAbortDialogOpen);
        Assert.DoesNotContain("Abort", device.Calls);

        vm.ConfirmAbortCommand.Execute(null);
        Assert.False(vm.IsAbortDialogOpen);
        Assert.Contains("Abort", device.Calls);
        Assert.Equal(RunState.Idle, vm.State);
    }

    [Fact]
    public void Cancel_abort_closes_dialog_without_aborting()
    {
        vm.StartCommand.Execute(null);
        vm.RequestAbortCommand.Execute(null);

        vm.CancelAbortCommand.Execute(null);

        Assert.False(vm.IsAbortDialogOpen);
        Assert.DoesNotContain("Abort", device.Calls);
        Assert.Equal(RunState.Running, vm.State);
    }

    [Fact]
    public void Snapshot_updates_progress_and_remaining_time()
    {
        device.Emit(RunState.Running, 30);

        Assert.Equal(0.5, vm.Progress);
        Assert.Equal("50%", vm.ProgressLabel);
        Assert.Equal("00:30", vm.RemainingLabel);
    }

    [Fact]
    public void Protocol_steps_track_elapsed_time()
    {
        device.Emit(RunState.Running, 10);

        Assert.True(vm.Steps[0].IsDone);
        Assert.False(vm.Steps[0].IsActive);
        Assert.True(vm.Steps[1].IsActive);
        Assert.False(vm.Steps[2].IsActive);
        Assert.False(vm.Steps[3].IsDone);
    }

    [Fact]
    public void Error_state_re_enables_start()
    {
        vm.StartCommand.Execute(null);
        vm.InjectErrorCommand.Execute(null);

        Assert.Equal(RunState.Error, vm.State);
        Assert.True(vm.StartCommand.CanExecute(null));
        Assert.False(vm.PauseResumeCommand.CanExecute(null));
    }
}
