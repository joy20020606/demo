using MultiplexAnalyzer.Hmi.Models;

namespace MultiplexAnalyzer.Hmi.Services;

public sealed class FakePlateService : IPlateService
{
    private const int MarkerCount = 8;
    private const double PositiveThreshold = 0.6;
    private const double HotWellRatio = 0.15;

    private readonly int seed;

    public FakePlateService(int seed = 42)
    {
        this.seed = seed;
    }

    public int Rows => 8;

    public int Columns => 12;

    public IReadOnlyList<Well> LoadPlate()
    {
        var random = new Random(seed);
        var wells = new List<Well>(Rows * Columns);

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                var isHot = random.NextDouble() < HotWellRatio;
                var markers = new List<MarkerResult>(MarkerCount);

                for (var index = 0; index < MarkerCount; index++)
                {
                    var baseline = random.NextDouble() * 0.55;
                    var signal = isHot && random.NextDouble() < 0.5
                        ? 0.6 + (random.NextDouble() * 0.4)
                        : baseline;

                    markers.Add(new MarkerResult(
                        $"M-{index + 1:00}",
                        Math.Round(signal, 3),
                        signal >= PositiveThreshold));
                }

                wells.Add(new Well(row, column, markers));
            }
        }

        return wells;
    }
}
