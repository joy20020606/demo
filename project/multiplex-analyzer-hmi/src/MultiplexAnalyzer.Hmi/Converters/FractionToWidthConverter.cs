using System.Globalization;
using System.Windows.Data;

namespace MultiplexAnalyzer.Hmi.Converters;

public sealed class FractionToWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var fraction = value is double d ? Math.Clamp(d, 0, 1) : 0;
        var max = parameter is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 100;

        return fraction * max;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
