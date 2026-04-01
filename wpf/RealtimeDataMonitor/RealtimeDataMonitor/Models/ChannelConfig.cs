using OxyPlot;

namespace RealtimeDataMonitor.Models;

public class ChannelConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OxyColor Color { get; set; }
    public double UpperThreshold { get; set; }
    public double LowerThreshold { get; set; }
    public bool IsEnabled { get; set; } = true;

    public static List<ChannelConfig> CreateDefaults() =>
    [
        new() { Id = 0, Name = "Channel A", Color = OxyColors.DodgerBlue,  UpperThreshold = 80, LowerThreshold = 20 },
        new() { Id = 1, Name = "Channel B", Color = OxyColors.OrangeRed,   UpperThreshold = 75, LowerThreshold = 25 },
        new() { Id = 2, Name = "Channel C", Color = OxyColors.MediumSeaGreen, UpperThreshold = 70, LowerThreshold = 30 },
        new() { Id = 3, Name = "Channel D", Color = OxyColors.Orchid,      UpperThreshold = 85, LowerThreshold = 15 },
    ];
}
