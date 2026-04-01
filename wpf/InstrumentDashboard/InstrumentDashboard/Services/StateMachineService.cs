using InstrumentDashboard.Models;

namespace InstrumentDashboard.Services;

public class StateChangedEventArgs : EventArgs
{
    public InstrumentStateBase PreviousState { get; }
    public InstrumentStateBase CurrentState { get; }

    public StateChangedEventArgs(InstrumentStateBase previous, InstrumentStateBase current)
    {
        PreviousState = previous;
        CurrentState = current;
    }
}

public class StateMachineService
{
    private InstrumentStateBase _currentState = new IdleState();

    public event EventHandler<StateChangedEventArgs>? StateChanged;

    public InstrumentStateBase CurrentState => _currentState;

    public bool TryTransition(InstrumentStateType target)
    {
        var allowed = IsTransitionAllowed(_currentState.StateType, target);
        if (!allowed) return false;

        var previous = _currentState;
        _currentState = CreateState(target);
        StateChanged?.Invoke(this, new StateChangedEventArgs(previous, _currentState));
        return true;
    }

    private static bool IsTransitionAllowed(InstrumentStateType from, InstrumentStateType to) =>
        (from, to) switch
        {
            (InstrumentStateType.Idle, InstrumentStateType.Initializing) => true,
            (InstrumentStateType.Initializing, InstrumentStateType.Running) => true,
            (InstrumentStateType.Initializing, InstrumentStateType.Idle) => true,
            (InstrumentStateType.Initializing, InstrumentStateType.Error) => true,
            (InstrumentStateType.Running, InstrumentStateType.Complete) => true,
            (InstrumentStateType.Running, InstrumentStateType.Idle) => true,
            (InstrumentStateType.Running, InstrumentStateType.Error) => true,
            (InstrumentStateType.Complete, InstrumentStateType.Idle) => true,
            (InstrumentStateType.Error, InstrumentStateType.Idle) => true,
            _ => false
        };

    private static InstrumentStateBase CreateState(InstrumentStateType type) => type switch
    {
        InstrumentStateType.Idle => new IdleState(),
        InstrumentStateType.Initializing => new InitializingState(),
        InstrumentStateType.Running => new RunningState(),
        InstrumentStateType.Complete => new CompleteState(),
        InstrumentStateType.Error => new ErrorState(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
