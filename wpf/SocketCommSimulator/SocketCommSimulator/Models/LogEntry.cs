namespace SocketCommSimulator.Models;

public enum MessageDirection
{
    Sent,
    Received,
    System
}

public enum DisplayFormat
{
    Ascii,
    Hex
}

public class LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public MessageDirection Direction { get; init; }
    public byte[] RawData { get; init; } = [];
    public string Source { get; init; } = string.Empty;

    public string DirectionSymbol => Direction switch
    {
        MessageDirection.Sent => ">>",
        MessageDirection.Received => "<<",
        MessageDirection.System => "--",
        _ => "??"
    };

    public string GetContent(DisplayFormat format) => format switch
    {
        DisplayFormat.Hex => BitConverter.ToString(RawData).Replace("-", " "),
        _ => System.Text.Encoding.UTF8.GetString(RawData)
    };

    public string Format(DisplayFormat format)
    {
        var content = GetContent(format);
        var prefix = Source.Length > 0 ? $"[{Source}] " : string.Empty;
        return $"[{Timestamp:HH:mm:ss.fff}] {DirectionSymbol} {prefix}{content}";
    }

    public static LogEntry CreateSystem(string message) => new()
    {
        Direction = MessageDirection.System,
        RawData = System.Text.Encoding.UTF8.GetBytes(message)
    };

    public static LogEntry CreateSent(byte[] data, string source = "") => new()
    {
        Direction = MessageDirection.Sent,
        RawData = data,
        Source = source
    };

    public static LogEntry CreateReceived(byte[] data, string source = "") => new()
    {
        Direction = MessageDirection.Received,
        RawData = data,
        Source = source
    };
}
