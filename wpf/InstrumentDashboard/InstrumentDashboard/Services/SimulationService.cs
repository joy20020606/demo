using InstrumentDashboard.Models;

namespace InstrumentDashboard.Services;

public class SensorData
{
    public double Temperature { get; set; }
    public double Pressure { get; set; }
    public double Vacuum { get; set; }
    public double Voltage { get; set; }
    public double Current { get; set; }
    public double Progress { get; set; }
}

public class SimulationService
{
    private readonly Random _rng = new();
    private double _progress;
    private InstrumentStateType _state = InstrumentStateType.Idle;

    public void SetState(InstrumentStateType state)
    {
        _state = state;
        if (state == InstrumentStateType.Idle || state == InstrumentStateType.Complete)
            _progress = 0;
    }

    public SensorData GetNextReading(InstrumentParameter parameters)
    {
        if (_state == InstrumentStateType.Running)
            _progress = Math.Min(_progress + _rng.NextDouble() * 0.5, 100.0);

        double Noise() => (_rng.NextDouble() - 0.5) * 2;

        return new SensorData
        {
            Temperature = parameters.TargetTemperature + Noise() * 0.5,
            Pressure = _state == InstrumentStateType.Running
                ? parameters.TargetPressure - _progress * 0.1 + Noise() * 0.2
                : parameters.TargetPressure + Noise() * 0.1,
            Vacuum = _state == InstrumentStateType.Running
                ? parameters.VacuumLevel * (1 + Noise() * 0.05)
                : parameters.VacuumLevel * 10,
            Voltage = _state == InstrumentStateType.Running
                ? parameters.Voltage + Noise() * 0.01
                : 0,
            Current = _state == InstrumentStateType.Running
                ? parameters.Current + Noise() * 0.001
                : 0,
            Progress = _progress
        };
    }
}
