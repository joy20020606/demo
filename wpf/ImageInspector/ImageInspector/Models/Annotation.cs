using System.Text.Json.Serialization;

namespace ImageInspector.Models;

public enum AnnotationShape
{
    Rectangle,
    Ellipse
}

public class Annotation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AnnotationShape Shape { get; set; } = AnnotationShape.Rectangle;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = "#FF0000";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public bool IsSelected { get; set; }
}
