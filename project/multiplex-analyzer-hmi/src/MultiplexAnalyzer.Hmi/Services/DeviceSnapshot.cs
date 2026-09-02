using MultiplexAnalyzer.Hmi.Models;

namespace MultiplexAnalyzer.Hmi.Services;

public sealed record DeviceSnapshot(RunState State, double ElapsedMinutes);
