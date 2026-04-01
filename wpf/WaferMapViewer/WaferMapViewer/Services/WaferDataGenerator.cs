using WaferMapViewer.Models;

namespace WaferMapViewer.Services;

public static class WaferDataGenerator
{
    private static readonly Random _rng = new(42);

    public static WaferMap Generate(int rows = 25, int cols = 25, double diameterMm = 300)
    {
        var wafer = new WaferMap
        {
            WaferId = $"WF-{_rng.Next(10000, 99999)}",
            LotId = $"LOT-{_rng.Next(1000, 9999)}",
            DiameterMm = diameterMm,
            DieSizeUmX = 10000,
            DieSizeUmY = 10000,
            Rows = rows,
            Cols = cols,
            InspectionTime = DateTime.Now
        };

        double centerR = (rows - 1) / 2.0;
        double centerC = (cols - 1) / 2.0;
        double radius = Math.Min(rows, cols) / 2.0 - 0.5;

        var hotZones = GenerateHotZones(rows, cols, 3);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double dist = Math.Sqrt(Math.Pow(r - centerR, 2) + Math.Pow(c - centerC, 2));

                if (dist > radius)
                    continue;

                bool isEdge = dist > radius - 1.0;
                var die = new DieInfo
                {
                    Row = r,
                    Col = c,
                    IsEdgeDie = isEdge
                };

                if (isEdge)
                {
                    die.Result = InspectionResult.EdgeDie;
                }
                else
                {
                    die.Result = PickResult(r, c, hotZones);
                    die.DefectDensity = die.Result == InspectionResult.Pass ? 0 : _rng.NextDouble() * 5;
                    die.DefectDescription = die.Result != InspectionResult.Pass
                        ? $"{die.Result} detected at die ({r},{c})"
                        : null;
                    die.Measurements["Thickness_nm"] = 200 + _rng.NextDouble() * 10;
                    die.Measurements["Roughness_nm"] = _rng.NextDouble() * 2;
                    die.Measurements["CD_nm"] = 65 + _rng.NextDouble() * 5;
                }

                wafer.Dies.Add(die);
            }
        }

        return wafer;
    }

    private static List<(double r, double c, double strength)> GenerateHotZones(int rows, int cols, int count)
    {
        var zones = new List<(double, double, double)>();
        for (int i = 0; i < count; i++)
        {
            zones.Add((_rng.NextDouble() * rows, _rng.NextDouble() * cols, _rng.NextDouble()));
        }
        return zones;
    }

    private static InspectionResult PickResult(int r, int c, List<(double r, double c, double strength)> hotZones)
    {
        double failProb = 0.05;
        foreach (var (hr, hc, strength) in hotZones)
        {
            double dist = Math.Sqrt(Math.Pow(r - hr, 2) + Math.Pow(c - hc, 2));
            failProb += strength * Math.Exp(-dist / 3.0) * 0.4;
        }
        failProb = Math.Min(failProb, 0.7);

        double roll = _rng.NextDouble();
        if (roll >= failProb)
            return InspectionResult.Pass;

        double defectRoll = _rng.NextDouble();
        return defectRoll switch
        {
            < 0.35 => InspectionResult.Fail,
            < 0.55 => InspectionResult.Scratch,
            < 0.72 => InspectionResult.Particle,
            < 0.85 => InspectionResult.Void,
            _      => InspectionResult.Crack
        };
    }
}
