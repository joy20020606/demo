namespace DefectClassifier.Models;

public class DefectRecord
{
    public int Id { get; set; }
    public DefectType Type { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string WaferId { get; set; } = string.Empty;
    public int Severity { get; set; }

    public double Area => Width * Height;
}
