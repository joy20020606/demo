using RealtimeDataMonitor.Models;

namespace RealtimeDataMonitor.Services;

public class AlarmService
{
    private readonly List<ChannelConfig> _channels;
    private readonly Dictionary<int, bool> _highAlarmActive = new();
    private readonly Dictionary<int, bool> _lowAlarmActive = new();

    public event Action<AlarmEvent>? AlarmTriggered;

    public AlarmService(List<ChannelConfig> channels)
    {
        _channels = channels;
        foreach (var ch in channels)
        {
            _highAlarmActive[ch.Id] = false;
            _lowAlarmActive[ch.Id] = false;
        }
    }

    public void CheckData(IReadOnlyList<SensorData> dataPoints)
    {
        foreach (var data in dataPoints)
        {
            var config = _channels.FirstOrDefault(c => c.Id == data.ChannelId);
            if (config is null || !config.IsEnabled) continue;

            if (data.Value > config.UpperThreshold && !_highAlarmActive[data.ChannelId])
            {
                _highAlarmActive[data.ChannelId] = true;
                AlarmTriggered?.Invoke(new AlarmEvent
                {
                    ChannelId = data.ChannelId,
                    ChannelName = config.Name,
                    Value = data.Value,
                    Threshold = config.UpperThreshold,
                    Severity = data.Value > config.UpperThreshold + 10
                        ? AlarmSeverity.Critical : AlarmSeverity.Warning,
                    IsHighAlarm = true,
                    Timestamp = data.Timestamp
                });
            }
            else if (data.Value <= config.UpperThreshold)
            {
                _highAlarmActive[data.ChannelId] = false;
            }

            if (data.Value < config.LowerThreshold && !_lowAlarmActive[data.ChannelId])
            {
                _lowAlarmActive[data.ChannelId] = true;
                AlarmTriggered?.Invoke(new AlarmEvent
                {
                    ChannelId = data.ChannelId,
                    ChannelName = config.Name,
                    Value = data.Value,
                    Threshold = config.LowerThreshold,
                    Severity = data.Value < config.LowerThreshold - 10
                        ? AlarmSeverity.Critical : AlarmSeverity.Warning,
                    IsHighAlarm = false,
                    Timestamp = data.Timestamp
                });
            }
            else if (data.Value >= config.LowerThreshold)
            {
                _lowAlarmActive[data.ChannelId] = false;
            }
        }
    }
}
