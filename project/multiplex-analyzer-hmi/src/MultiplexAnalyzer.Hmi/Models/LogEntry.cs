namespace MultiplexAnalyzer.Hmi.Models;

public sealed record LogEntry(DateTime Timestamp, LogLevel Level, string Source, string Message)
{
    public string TimeLabel => Timestamp.ToString("HH:mm:ss");
}
