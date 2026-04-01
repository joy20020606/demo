namespace RecipeEditor.Models;

public class StepParameter
{
    public string Name { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public double MinValue { get; set; }
    public double MaxValue { get; set; }

    public StepParameter Clone() => new()
    {
        Name = Name,
        Value = Value,
        Unit = Unit,
        MinValue = MinValue,
        MaxValue = MaxValue
    };
}
