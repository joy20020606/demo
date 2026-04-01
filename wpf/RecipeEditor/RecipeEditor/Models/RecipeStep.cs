namespace RecipeEditor.Models;

public enum StepType
{
    Deposition,
    Etch,
    Clean,
    Anneal,
    Transfer,
    Wait
}

public class RecipeStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "New Step";
    public StepType StepType { get; set; } = StepType.Wait;
    public double DurationSeconds { get; set; } = 60;
    public List<StepParameter> Parameters { get; set; } = new();

    public RecipeStep Clone() => new()
    {
        Id = Id,
        Name = Name,
        StepType = StepType,
        DurationSeconds = DurationSeconds,
        Parameters = Parameters.Select(p => p.Clone()).ToList()
    };
}
