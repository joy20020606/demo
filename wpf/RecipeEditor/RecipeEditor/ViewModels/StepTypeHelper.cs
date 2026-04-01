using RecipeEditor.Models;

namespace RecipeEditor.ViewModels;

public static class StepTypeValues
{
    public static StepType[] All { get; } = (StepType[])Enum.GetValues(typeof(StepType));
}
