namespace RealtimeDataMonitor.Models;

public class SensorData
{
    public DateTime Timestamp { get; init; }
    public double Value { get; init; }
    public int ChannelId { get; init; }

    public SensorData(int channelId, double value, DateTime? timestamp = null)
    {
        ChannelId = channelId;
        Value = value;
        Timestamp = timestamp ?? DateTime.Now;
    }
}
