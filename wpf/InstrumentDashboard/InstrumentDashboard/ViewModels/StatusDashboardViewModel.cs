using CommunityToolkit.Mvvm.ComponentModel;

namespace InstrumentDashboard.ViewModels;

public partial class StatusDashboardViewModel : ObservableObject
{
    [ObservableProperty] private double _temperature;
    [ObservableProperty] private double _pressure;
    [ObservableProperty] private double _vacuum;
    [ObservableProperty] private double _voltage;
    [ObservableProperty] private double _current;
    [ObservableProperty] private double _progress;

    public void Update(double temperature, double pressure, double vacuum,
        double voltage, double current, double progress)
    {
        Temperature = temperature;
        Pressure = pressure;
        Vacuum = vacuum;
        Voltage = voltage;
        Current = current;
        Progress = progress;
    }
}
