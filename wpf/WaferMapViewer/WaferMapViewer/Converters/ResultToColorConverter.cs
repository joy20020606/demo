using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WaferMapViewer.Models;
using WaferMapViewer.Services;

namespace WaferMapViewer.Converters;

public class ResultToColorConverter : IValueConverter
{
    private static readonly IColorMap _map = ColorMapFactory.Create("Standard");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is InspectionResult result)
            return new SolidColorBrush(_map.GetColor(result));
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is true;
        if (parameter is string p && p == "Inverse") flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class YieldToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double yield)
        {
            return yield >= 90 ? new SolidColorBrush(Color.FromRgb(0x2D, 0xC6, 0x53))
                 : yield >= 70 ? new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00))
                               : new SolidColorBrush(Color.FromRgb(0xE6, 0x39, 0x46));
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
