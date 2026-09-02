using MultiplexAnalyzer.Hmi.Models;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public sealed class WellViewModel
{
    public WellViewModel(Well well)
    {
        Row = well.Row;
        Column = well.Column;
        Label = well.Label;
        Markers = well.Markers.Select(marker => new MarkerViewModel(marker)).ToList();
        PositiveCount = well.PositiveCount;
        Summary = Markers.Count == 0 ? 0 : (double)PositiveCount / Markers.Count;
    }

    public int Row { get; }

    public int Column { get; }

    public string Label { get; }

    public IReadOnlyList<MarkerViewModel> Markers { get; }

    public int PositiveCount { get; }

    public double Summary { get; }

    public string SummaryLabel => $"{PositiveCount} of {Markers.Count} markers positive";
}
