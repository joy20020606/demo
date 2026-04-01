using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DefectClassifier.Models;

namespace DefectClassifier.Converters;

public class DefectTypeToColorConverter : IValueConverter
{
    private static readonly Dictionary<DefectType, Brush> Brushes = new()
    {
        [DefectType.Scratch]       = new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)),
        [DefectType.Particle]      = new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12)),
        [DefectType.PatternDefect] = new SolidColorBrush(Color.FromRgb(0x9B, 0x59, 0xB6)),
        [DefectType.Pit]           = new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
        [DefectType.Bridge]        = new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x71)),
        [DefectType.Unknown]       = new SolidColorBrush(Color.FromRgb(0x95, 0xA5, 0xA6)),
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DefectType t && Brushes.TryGetValue(t, out var b) ? b : System.Windows.Media.Brushes.Gray;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
