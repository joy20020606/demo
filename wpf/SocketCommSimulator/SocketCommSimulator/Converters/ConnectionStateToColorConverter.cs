using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SocketCommSimulator.Models;

namespace SocketCommSimulator.Converters;

public class ConnectionStateToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ConnectionState state)
        {
            return state switch
            {
                ConnectionState.Connected => Brushes.LimeGreen,
                ConnectionState.Listening => Brushes.DodgerBlue,
                ConnectionState.Connecting => Brushes.Orange,
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
