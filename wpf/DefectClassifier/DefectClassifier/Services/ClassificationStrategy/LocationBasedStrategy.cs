using DefectClassifier.Models;

namespace DefectClassifier.Services.ClassificationStrategy;

public class LocationBasedStrategy : IClassificationStrategy
{
    public string Name => "位置分類";
    public string Description => "依缺陷在晶圓上的座標位置進行自動分類";

    public ClassificationResult Classify(DefectRecord defect)
    {
        var distFromCenter = Math.Sqrt(defect.X * defect.X + defect.Y * defect.Y);
        DefectType type;
        double confidence;

        if (distFromCenter < 30)
        {
            type = DefectType.PatternDefect;
            confidence = 0.80;
        }
        else if (distFromCenter < 80)
        {
            type = DefectType.Particle;
            confidence = 0.75;
        }
        else if (distFromCenter < 130)
        {
            type = DefectType.Scratch;
            confidence = 0.70;
        }
        else
        {
            type = DefectType.Bridge;
            confidence = 0.65;
        }

        return new ClassificationResult
        {
            DefectId = defect.Id,
            SuggestedType = type,
            Confidence = confidence,
            StrategyName = Name
        };
    }
}
