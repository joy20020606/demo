namespace WaferMapViewer.Models;

public class WaferMap
{
    public string WaferId { get; set; } = string.Empty;
    public string LotId { get; set; } = string.Empty;
    public double DiameterMm { get; set; }
    public double DieSizeUmX { get; set; }
    public double DieSizeUmY { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    public List<DieInfo> Dies { get; set; } = new();
    public DateTime InspectionTime { get; set; }

    public DieInfo? GetDie(int row, int col) =>
        Dies.FirstOrDefault(d => d.Row == row && d.Col == col);

    public int TotalDies => Dies.Count(d => d.Result != InspectionResult.NotTested && !d.IsEdgeDie);
    public int PassCount => Dies.Count(d => d.Result == InspectionResult.Pass);
    public int FailCount => Dies.Count(d => d.Result == InspectionResult.Fail ||
                                            d.Result == InspectionResult.Scratch ||
                                            d.Result == InspectionResult.Particle ||
                                            d.Result == InspectionResult.Void ||
                                            d.Result == InspectionResult.Crack);
    public double Yield => TotalDies > 0 ? (double)PassCount / TotalDies * 100.0 : 0;
}
