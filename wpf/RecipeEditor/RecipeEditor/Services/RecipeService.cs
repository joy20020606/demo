using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RecipeEditor.Models;

namespace RecipeEditor.Services;

public class RecipeService
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(Recipe recipe, string filePath)
    {
        recipe.ModifiedAt = DateTime.Now;
        var json = JsonSerializer.Serialize(recipe, _options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Recipe> LoadAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var recipe = JsonSerializer.Deserialize<Recipe>(json, _options);
        return recipe ?? throw new InvalidDataException("Invalid recipe file.");
    }

    public string Serialize(Recipe recipe) => JsonSerializer.Serialize(recipe, _options);

    public Recipe Deserialize(string json)
    {
        var recipe = JsonSerializer.Deserialize<Recipe>(json, _options);
        return recipe ?? throw new InvalidDataException("Invalid recipe data.");
    }
}
