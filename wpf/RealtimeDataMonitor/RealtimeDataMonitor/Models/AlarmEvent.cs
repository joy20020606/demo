namespace RealtimeDataMonitor.Models;

public enum AlarmSeverity
{
    Warning,
    Critical
}

public class AlarmEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public int ChannelId { get; init; }
    public string ChannelName { get; init; } = string.Empty;
    public double Value { get; init; }
    public double Threshold { get; init; }
    public AlarmSeverity Severity { get; init; }
    public bool IsHighAlarm { get; init; }
    public bool IsAcknowledged { get; set; }

    public string Description =>
        $"{ChannelName}: {Value:F2} {(IsHighAlarm ? ">" : "<")} {Threshold:F2}";
}
