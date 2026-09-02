using MultiplexAnalyzer.Hmi.Models;

namespace MultiplexAnalyzer.Hmi.Services;

public interface IPlateService
{
    int Rows { get; }

    int Columns { get; }

    IReadOnlyList<Well> LoadPlate();
}
