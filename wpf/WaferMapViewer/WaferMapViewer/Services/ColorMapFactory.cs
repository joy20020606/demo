using System.Windows.Media;
using WaferMapViewer.Models;

namespace WaferMapViewer.Services;

public interface IColorMap
{
    string Name { get; }
    Color GetColor(InspectionResult result);
}

public abstract class ColorMapBase : IColorMap
{
    public abstract string Name { get; }
    public abstract Color GetColor(InspectionResult result);
}

public class StandardColorMap : ColorMapBase
{
    public override string Name => "Standard";

    public override Color GetColor(InspectionResult result) => result switch
    {
        InspectionResult.Pass       => Color.FromRgb(0x2D, 0xC6, 0x53),
        InspectionResult.Fail       => Color.FromRgb(0xE6, 0x39, 0x46),
        InspectionResult.Scratch    => Color.FromRgb(0xFF, 0xA5, 0x00),
        InspectionResult.Particle   => Color.FromRgb(0xFF, 0xD7, 0x00),
        InspectionResult.EdgeDie    => Color.FromRgb(0x80, 0x80, 0x80),
        InspectionResult.Void       => Color.FromRgb(0x8B, 0x00, 0xFF),
        InspectionResult.Crack      => Color.FromRgb(0xFF, 0x45, 0x00),
        InspectionResult.NotTested  => Color.FromRgb(0x22, 0x22, 0x22),
        _                           => Colors.Black
    };
}

public class MonochromeColorMap : ColorMapBase
{
    public override string Name => "Monochrome";

    public override Color GetColor(InspectionResult result) => result switch
    {
        InspectionResult.Pass       => Color.FromRgb(0xEE, 0xEE, 0xEE),
        InspectionResult.Fail       => Color.FromRgb(0x11, 0x11, 0x11),
        InspectionResult.Scratch    => Color.FromRgb(0x66, 0x66, 0x66),
        InspectionResult.Particle   => Color.FromRgb(0x88, 0x88, 0x88),
        InspectionResult.EdgeDie    => Color.FromRgb(0xAA, 0xAA, 0xAA),
        InspectionResult.Void       => Color.FromRgb(0x33, 0x33, 0x33),
        InspectionResult.Crack      => Color.FromRgb(0x44, 0x44, 0x44),
        InspectionResult.NotTested  => Color.FromRgb(0x1A, 0x1A, 0x1A),
        _                           => Colors.Black
    };
}

public class HeatMapColorMap : ColorMapBase
{
    public override string Name => "Heat Map";

    public override Color GetColor(InspectionResult result) => result switch
    {
        InspectionResult.Pass       => Color.FromRgb(0x00, 0x3F, 0x5C),
        InspectionResult.Fail       => Color.FromRgb(0xFF, 0x00, 0x00),
        InspectionResult.Scratch    => Color.FromRgb(0xFF, 0x6E, 0x54),
        InspectionResult.Particle   => Color.FromRgb(0xFF, 0xD1, 0x66),
        InspectionResult.EdgeDie    => Color.FromRgb(0x66, 0x5C, 0x54),
        InspectionResult.Void       => Color.FromRgb(0xBC, 0x00, 0x6C),
        InspectionResult.Crack      => Color.FromRgb(0xFF, 0x45, 0x00),
        InspectionResult.NotTested  => Color.FromRgb(0x1A, 0x1A, 0x2E),
        _                           => Colors.Black
    };
}

public static class ColorMapFactory
{
    private static readonly Dictionary<string, IColorMap> _maps = new()
    {
        { "Standard",   new StandardColorMap() },
        { "Monochrome", new MonochromeColorMap() },
        { "Heat Map",   new HeatMapColorMap() }
    };

    public static IColorMap Create(string name) =>
        _maps.TryGetValue(name, out var map) ? map : new StandardColorMap();

    public static IReadOnlyList<string> AvailableNames => _maps.Keys.ToList();
}
