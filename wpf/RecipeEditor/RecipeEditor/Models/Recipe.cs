namespace RecipeEditor.Models;

public class Recipe
{
    public string Name { get; set; } = "New Recipe";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "";
    public string Author { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
    public List<RecipeStep> Steps { get; set; } = new();

    public Recipe Clone() => new()
    {
        Name = Name,
        Version = Version,
        Description = Description,
        Author = Author,
        CreatedAt = CreatedAt,
        ModifiedAt = ModifiedAt,
        Steps = Steps.Select(s => s.Clone()).ToList()
    };
}
