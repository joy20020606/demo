using CommunityToolkit.Mvvm.ComponentModel;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public sealed partial class SettingsViewModel : PageViewModelBase
{
    [ObservableProperty]
    private string selectedReadMode = "Standard";

    [ObservableProperty]
    private bool autoExportEnabled = true;

    [ObservableProperty]
    private bool soundAlertsEnabled;

    [ObservableProperty]
    private double incubationTemperature = 37;

    [ObservableProperty]
    private double washCycles = 3;

    public override string Title => "Settings";

    public override string Subtitle => "Custom controls, design tokens, theme switching";

    public string[] ReadModes { get; } = ["Fast", "Standard", "High Sensitivity"];
}
