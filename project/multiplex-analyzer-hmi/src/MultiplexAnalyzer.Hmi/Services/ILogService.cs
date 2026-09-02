using MultiplexAnalyzer.Hmi.Models;

namespace MultiplexAnalyzer.Hmi.Services;

public interface ILogService
{
    IReadOnlyList<LogEntry> Load(int count);
}
