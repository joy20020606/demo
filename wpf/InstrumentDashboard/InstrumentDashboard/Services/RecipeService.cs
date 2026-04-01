using System.IO;
using System.Text.Json;
using InstrumentDashboard.Models;

namespace InstrumentDashboard.Services;

public class RecipeService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public void Save(Recipe recipe, string filePath)
    {
        var json = JsonSerializer.Serialize(recipe, Options);
        File.WriteAllText(filePath, json);
    }

    public Recipe? Load(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Recipe>(json);
    }

    public IEnumerable<Recipe> LoadAll(string directory)
    {
        if (!Directory.Exists(directory)) yield break;
        foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
        {
            var recipe = Load(file);
            if (recipe is not null) yield return recipe;
        }
    }
}
