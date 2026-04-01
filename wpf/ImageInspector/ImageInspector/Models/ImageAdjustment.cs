namespace ImageInspector.Models;

public class ImageAdjustment
{
    public double Brightness { get; set; } = 0.0;
    public double Contrast { get; set; } = 1.0;
    public double Gamma { get; set; } = 1.0;

    public static ImageAdjustment Default => new ImageAdjustment();
}
