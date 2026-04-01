using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InstrumentDashboard.Converters;

public class LogLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string level ? level switch
        {
            "WARN" => new SolidColorBrush(Color.FromRgb(255, 165, 0)),
            "ERROR" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
            _ => new SolidColorBrush(Color.FromRgb(200, 200, 200))
        } : Brushes.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
