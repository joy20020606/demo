using DefectClassifier.Models;

namespace DefectClassifier.Services.ClassificationStrategy;

public interface IClassificationStrategy
{
    string Name { get; }
    string Description { get; }
    ClassificationResult Classify(DefectRecord defect);
}
