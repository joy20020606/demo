namespace MultiplexAnalyzer.Hmi.Models;

public sealed record Well(int Row, int Column, IReadOnlyList<MarkerResult> Markers)
{
    public string Label => $"{(char)(65 + Row)}{Column + 1}";

    public int PositiveCount => Markers.Count(marker => marker.IsPositive);
}
