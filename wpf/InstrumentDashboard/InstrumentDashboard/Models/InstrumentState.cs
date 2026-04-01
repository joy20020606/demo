namespace InstrumentDashboard.Models;

public enum InstrumentStateType
{
    Idle,
    Initializing,
    Running,
    Complete,
    Error
}

public abstract class InstrumentStateBase
{
    public abstract InstrumentStateType StateType { get; }
    public abstract string DisplayName { get; }
    public abstract bool CanStart { get; }
    public abstract bool CanStop { get; }
    public abstract bool CanReset { get; }
}

public class IdleState : InstrumentStateBase
{
    public override InstrumentStateType StateType => InstrumentStateType.Idle;
    public override string DisplayName => "Idle";
    public override bool CanStart => true;
    public override bool CanStop => false;
    public override bool CanReset => false;
}

public class InitializingState : InstrumentStateBase
{
    public override InstrumentStateType StateType => InstrumentStateType.Initializing;
    public override string DisplayName => "Initializing";
    public override bool CanStart => false;
    public override bool CanStop => true;
    public override bool CanReset => false;
}

public class RunningState : InstrumentStateBase
{
    public override InstrumentStateType StateType => InstrumentStateType.Running;
    public override string DisplayName => "Running";
    public override bool CanStart => false;
    public override bool CanStop => true;
    public override bool CanReset => false;
}

public class CompleteState : InstrumentStateBase
{
    public override InstrumentStateType StateType => InstrumentStateType.Complete;
    public override string DisplayName => "Complete";
    public override bool CanStart => false;
    public override bool CanStop => false;
    public override bool CanReset => true;
}

public class ErrorState : InstrumentStateBase
{
    public override InstrumentStateType StateType => InstrumentStateType.Error;
    public override string DisplayName => "Error";
    public override bool CanStart => false;
    public override bool CanStop => false;
    public override bool CanReset => true;
}
