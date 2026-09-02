using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MultiplexAnalyzer.Hmi.Converters;

public sealed class ResourceKeyToGeometryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key)
        {
            return Geometry.Empty;
        }

        return Application.Current.TryFindResource(key) as Geometry ?? Geometry.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
