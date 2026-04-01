using System.IO;
using System.Text;
using RealtimeDataMonitor.Models;

namespace RealtimeDataMonitor.Services;

public class CsvExportService
{
    public async Task ExportAsync(
        IEnumerable<SensorData> data,
        List<ChannelConfig> channels,
        string filePath,
        CancellationToken token = default)
    {
        var channelMap = channels.ToDictionary(c => c.Id, c => c.Name);

        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,ChannelId,ChannelName,Value");

        foreach (var d in data.OrderBy(x => x.Timestamp).ThenBy(x => x.ChannelId))
        {
            token.ThrowIfCancellationRequested();
            var name = channelMap.TryGetValue(d.ChannelId, out var n) ? n : $"Ch{d.ChannelId}";
            sb.AppendLine($"{d.Timestamp:yyyy-MM-dd HH:mm:ss.fff},{d.ChannelId},{name},{d.Value:F4}");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8, token);
    }

    public async Task ExportAlarmsAsync(
        IEnumerable<AlarmEvent> alarms,
        string filePath,
        CancellationToken token = default)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,ChannelId,ChannelName,Value,Threshold,IsHighAlarm,Severity,IsAcknowledged,Description");

        foreach (var a in alarms.OrderBy(x => x.Timestamp))
        {
            token.ThrowIfCancellationRequested();
            sb.AppendLine($"{a.Timestamp:yyyy-MM-dd HH:mm:ss.fff},{a.ChannelId},{a.ChannelName}," +
                          $"{a.Value:F4},{a.Threshold:F4},{a.IsHighAlarm},{a.Severity},{a.IsAcknowledged}," +
                          $"\"{a.Description}\"");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8, token);
    }
}
