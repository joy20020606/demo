namespace WaferMapViewer.Models;

public class DieInfo
{
    public int Row { get; set; }
    public int Col { get; set; }
    public InspectionResult Result { get; set; }
    public double DefectDensity { get; set; }
    public string? DefectDescription { get; set; }
    public Dictionary<string, object> Measurements { get; set; } = new();
    public bool IsEdgeDie { get; set; }
}
