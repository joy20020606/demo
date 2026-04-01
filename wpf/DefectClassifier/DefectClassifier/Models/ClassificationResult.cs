namespace DefectClassifier.Models;

public class ClassificationResult
{
    public int DefectId { get; set; }
    public DefectType SuggestedType { get; set; }
    public double Confidence { get; set; }
    public string StrategyName { get; set; } = string.Empty;
}
