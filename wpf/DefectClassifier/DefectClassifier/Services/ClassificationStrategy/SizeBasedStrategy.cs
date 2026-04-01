using DefectClassifier.Models;

namespace DefectClassifier.Services.ClassificationStrategy;

public class SizeBasedStrategy : IClassificationStrategy
{
    public string Name => "尺寸分類";
    public string Description => "依缺陷面積大小與長寬比進行自動分類";

    public ClassificationResult Classify(DefectRecord defect)
    {
        var area = defect.Area;
        DefectType type;
        double confidence;

        bool isLinear = defect.Height > 0 && defect.Width > 0 &&
                        (defect.Width / defect.Height > 5 || defect.Height / defect.Width > 5);

        if (isLinear)
        {
            type = DefectType.Scratch;
            confidence = 0.90;
        }
        else if (area < 10)
        {
            type = DefectType.Particle;
            confidence = 0.85;
        }
        else if (area < 100)
        {
            type = DefectType.Pit;
            confidence = 0.75;
        }
        else if (area < 500)
        {
            type = DefectType.PatternDefect;
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
