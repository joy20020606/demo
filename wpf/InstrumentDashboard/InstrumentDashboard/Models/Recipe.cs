namespace InstrumentDashboard.Models;

public class Recipe
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public InstrumentParameter Parameters { get; set; } = new();
}
