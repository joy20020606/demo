using CommunityToolkit.Mvvm.ComponentModel;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public sealed partial class ProtocolStepViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private bool isDone;

    public ProtocolStepViewModel(string name, double durationMinutes)
    {
        Name = name;
        DurationMinutes = durationMinutes;
    }

    public string Name { get; }

    public double DurationMinutes { get; }

    public string DurationLabel => $"{DurationMinutes:0} min";
}
