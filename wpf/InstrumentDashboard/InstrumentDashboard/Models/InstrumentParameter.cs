namespace InstrumentDashboard.Models;

public class InstrumentParameter
{
    public double Voltage { get; set; } = 5.0;
    public double Current { get; set; } = 0.1;
    public double ScanSpeed { get; set; } = 100.0;
    public double SamplingRate { get; set; } = 1000.0;
    public double TargetTemperature { get; set; } = 25.0;
    public double TargetPressure { get; set; } = 101.325;
    public double VacuumLevel { get; set; } = 1e-4;
    public int ScanCount { get; set; } = 10;
}
