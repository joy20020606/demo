using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BatchProcessManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public TaskQueueViewModel TaskQueue { get; } = new();
    public ExecutionViewModel Execution { get; } = new();
    public ReportViewModel Report { get; } = new();

    [ObservableProperty] private int _selectedTabIndex = 0;

    public MainViewModel()
    {
        Execution.ExecutionCompleted += OnExecutionCompleted;
    }

    [RelayCommand]
    private async Task StartBatch()
    {
        if (!TaskQueue.Tasks.Any()) return;

        TaskQueue.ResetTasksForNewRun();
        SelectedTabIndex = 1;

        await Execution.StartAsync(TaskQueue.Tasks);
    }

    private void OnExecutionCompleted()
    {
        Report.UpdateResults(Execution.Results);
        SelectedTabIndex = 2;
    }
}
