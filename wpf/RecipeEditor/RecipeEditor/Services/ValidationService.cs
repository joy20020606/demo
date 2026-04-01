using RecipeEditor.Models;

namespace RecipeEditor.Services;

public enum ValidationSeverity { Error, Warning, Info }

public class ValidationResult
{
    public string Message { get; set; } = "";
    public ValidationSeverity Severity { get; set; }
    public string? StepName { get; set; }
    public string? ParameterName { get; set; }
}

public class ValidationService
{
    public List<ValidationResult> Validate(Recipe recipe)
    {
        var results = new List<ValidationResult>();

        if (string.IsNullOrWhiteSpace(recipe.Name))
            results.Add(new ValidationResult { Message = "Recipe name is required.", Severity = ValidationSeverity.Error });

        if (string.IsNullOrWhiteSpace(recipe.Version))
            results.Add(new ValidationResult { Message = "Recipe version is required.", Severity = ValidationSeverity.Error });

        if (recipe.Steps.Count == 0)
        {
            results.Add(new ValidationResult { Message = "Recipe must have at least one step.", Severity = ValidationSeverity.Warning });
            return results;
        }

        var duplicateNames = recipe.Steps
            .GroupBy(s => s.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);
        foreach (var dup in duplicateNames)
            results.Add(new ValidationResult
            {
                Message = $"Duplicate step name: \"{dup.Key}\".",
                Severity = ValidationSeverity.Error
            });

        foreach (var step in recipe.Steps)
        {
            if (string.IsNullOrWhiteSpace(step.Name))
                results.Add(new ValidationResult
                {
                    Message = "A step has no name.",
                    Severity = ValidationSeverity.Error,
                    StepName = step.Name
                });

            if (step.DurationSeconds <= 0)
                results.Add(new ValidationResult
                {
                    Message = $"Step \"{step.Name}\": Duration must be greater than 0.",
                    Severity = ValidationSeverity.Error,
                    StepName = step.Name
                });

            foreach (var param in step.Parameters)
            {
                if (param.Value < param.MinValue || param.Value > param.MaxValue)
                    results.Add(new ValidationResult
                    {
                        Message = $"Step \"{step.Name}\" / {param.Name}: value {param.Value} {param.Unit} is out of range [{param.MinValue}, {param.MaxValue}].",
                        Severity = ValidationSeverity.Error,
                        StepName = step.Name,
                        ParameterName = param.Name
                    });
            }
        }

        var hasDeposition = recipe.Steps.Any(s => s.StepType == StepType.Deposition);
        var cleanBeforeDeposition = recipe.Steps
            .TakeWhile(s => s.StepType != StepType.Deposition)
            .Any(s => s.StepType == StepType.Clean);
        if (hasDeposition && !cleanBeforeDeposition)
            results.Add(new ValidationResult
            {
                Message = "Recommendation: add a Clean step before the first Deposition step.",
                Severity = ValidationSeverity.Warning
            });

        var totalDuration = recipe.Steps.Sum(s => s.DurationSeconds);
        if (totalDuration > 7200)
            results.Add(new ValidationResult
            {
                Message = $"Total recipe duration is {totalDuration / 60:F0} min. Consider splitting into sub-recipes.",
                Severity = ValidationSeverity.Info
            });

        if (results.Count == 0)
            results.Add(new ValidationResult
            {
                Message = "All checks passed.",
                Severity = ValidationSeverity.Info
            });

        return results;
    }
}
