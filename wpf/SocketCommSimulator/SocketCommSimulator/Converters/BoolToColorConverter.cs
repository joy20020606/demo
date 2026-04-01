using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SocketCommSimulator.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object TrueValue { get; set; } = Brushes.Green;
    public object FalseValue { get; set; } = Brushes.Red;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? TrueValue : FalseValue;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
