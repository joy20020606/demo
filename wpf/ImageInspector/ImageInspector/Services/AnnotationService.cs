using System.IO;
using System.Text.Json;
using ImageInspector.Models;

namespace ImageInspector.Services;

public class AnnotationService
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public void Export(IEnumerable<Annotation> annotations, string filePath)
    {
        var json = JsonSerializer.Serialize(annotations.ToList(), _options);
        File.WriteAllText(filePath, json);
    }

    public List<Annotation> Import(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<Annotation>>(json, _options) ?? new List<Annotation>();
    }
}
