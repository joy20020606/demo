using System.Windows.Media;

namespace DefectClassifier.ViewModels;

public class ChartItem
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public Brush BarBrush { get; set; } = Brushes.SteelBlue;
    public double BarHeight { get; set; }
}
