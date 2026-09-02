using MultiplexAnalyzer.Hmi.Models;

namespace MultiplexAnalyzer.Hmi.Services;

public sealed class FakeLogService : ILogService
{
    private static readonly string[] Sources = ["Incubator", "Washer", "Reader", "PlateLoader", "System"];

    private static readonly string[] InfoMessages =
    [
        "Temperature stable at 37.0 C",
        "Wash cycle {0} of 3 completed",
        "Plate barcode scanned",
        "Image captured for well {1}",
        "Protocol step advanced",
        "Reagent volume check passed",
        "Self-test completed",
        "Door closed"
    ];

    private static readonly string[] WarningMessages =
    [
        "Temperature drift 0.4 C, compensating",
        "Wash pressure below nominal",
        "Well {1} signal near threshold",
        "Reader lamp at 82% intensity",
        "Retrying barcode scan"
    ];

    private static readonly string[] ErrorMessages =
    [
        "Incubator heater timeout",
        "Wash pump stalled",
        "Well {1} image decode failed",
        "Door opened during run",
        "Reader calibration mismatch"
    ];

    private readonly int seed;

    public FakeLogService(int seed = 7)
    {
        this.seed = seed;
    }

    public IReadOnlyList<LogEntry> Load(int count)
    {
        var random = new Random(seed);
        var entries = new List<LogEntry>(count);
        var timestamp = DateTime.Now;

        for (var index = 0; index < count; index++)
        {
            var roll = random.NextDouble();
            var level = roll < 0.05 ? LogLevel.Error : roll < 0.20 ? LogLevel.Warning : LogLevel.Info;
            var pool = level switch
            {
                LogLevel.Error => ErrorMessages,
                LogLevel.Warning => WarningMessages,
                _ => InfoMessages
            };

            var well = $"{(char)('A' + random.Next(8))}{random.Next(1, 13)}";
            var message = string.Format(pool[random.Next(pool.Length)], random.Next(1, 4), well);
            var source = Sources[random.Next(Sources.Length)];

            entries.Add(new LogEntry(timestamp, level, source, message));
            timestamp = timestamp.AddSeconds(-random.Next(2, 40));
        }

        return entries;
    }
}
