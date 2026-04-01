using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstrumentDashboard.Models;
using InstrumentDashboard.Services;

namespace InstrumentDashboard.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly StateMachineService _stateMachine;
    private readonly SimulationService _simulation;
    private readonly DispatcherTimer _timer;

    [ObservableProperty] private string _stateDisplayName = "Idle";
    [ObservableProperty] private bool _canStart;
    [ObservableProperty] private bool _canStop;
    [ObservableProperty] private bool _canReset;
    [ObservableProperty] private string _stateColor = "#4CAF50";

    public ParameterPanelViewModel ParameterPanel { get; }
    public StatusDashboardViewModel StatusDashboard { get; }
    public LogViewModel Log { get; }

    public MainViewModel()
    {
        var recipeService = new RecipeService();
        _stateMachine = new StateMachineService();
        _simulation = new SimulationService();

        ParameterPanel = new ParameterPanelViewModel(recipeService);
        StatusDashboard = new StatusDashboardViewModel();
        Log = new LogViewModel();

        _stateMachine.StateChanged += OnStateChanged;
        UpdateStateBindings(_stateMachine.CurrentState);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        Log.AddEntry("Application started.", "INFO");
    }

    private void OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        _simulation.SetState(e.CurrentState.StateType);
        UpdateStateBindings(e.CurrentState);
        Log.AddEntry($"State changed: {e.PreviousState.DisplayName} → {e.CurrentState.DisplayName}", "INFO");

        if (e.CurrentState.StateType == InstrumentStateType.Initializing)
            ScheduleTransition(InstrumentStateType.Running, 2000);
        else if (e.CurrentState.StateType == InstrumentStateType.Running)
            MonitorProgress();
    }

    private void UpdateStateBindings(InstrumentStateBase state)
    {
        StateDisplayName = state.DisplayName;
        CanStart = state.CanStart;
        CanStop = state.CanStop;
        CanReset = state.CanReset;
        StateColor = state.StateType switch
        {
            InstrumentStateType.Idle => "#4CAF50",
            InstrumentStateType.Initializing => "#FF9800",
            InstrumentStateType.Running => "#2196F3",
            InstrumentStateType.Complete => "#9C27B0",
            InstrumentStateType.Error => "#F44336",
            _ => "#4CAF50"
        };
    }

    private void ScheduleTransition(InstrumentStateType target, int delayMs)
    {
        Task.Delay(delayMs).ContinueWith(_ =>
            App.Current.Dispatcher.Invoke(() => _stateMachine.TryTransition(target)));
    }

    private bool _monitoringProgress;
    private void MonitorProgress()
    {
        if (_monitoringProgress) return;
        _monitoringProgress = true;
        Task.Run(async () =>
        {
            while (_stateMachine.CurrentState.StateType == InstrumentStateType.Running)
            {
                await Task.Delay(500);
                if (StatusDashboard.Progress >= 100)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _stateMachine.TryTransition(InstrumentStateType.Complete);
                        _monitoringProgress = false;
                    });
                    return;
                }
            }
            _monitoringProgress = false;
        });
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var parameters = ParameterPanel.GetParameters();
        var data = _simulation.GetNextReading(parameters);
        StatusDashboard.Update(data.Temperature, data.Pressure, data.Vacuum,
            data.Voltage, data.Current, data.Progress);
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        Log.AddEntry("Starting instrument initialization...", "INFO");
        _stateMachine.TryTransition(InstrumentStateType.Initializing);
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        Log.AddEntry("Operation stopped by user.", "WARN");
        _stateMachine.TryTransition(InstrumentStateType.Idle);
    }

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        Log.AddEntry("Instrument reset.", "INFO");
        _stateMachine.TryTransition(InstrumentStateType.Idle);
    }

    partial void OnCanStartChanged(bool value) => StartCommand.NotifyCanExecuteChanged();
    partial void OnCanStopChanged(bool value) => StopCommand.NotifyCanExecuteChanged();
    partial void OnCanResetChanged(bool value) => ResetCommand.NotifyCanExecuteChanged();
}
