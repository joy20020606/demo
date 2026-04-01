using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using BatchProcessManager.Models;

namespace BatchProcessManager.Converters;

public class BatchTaskStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BatchTaskStatus status)
        {
            return status switch
            {
                BatchTaskStatus.Completed => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                BatchTaskStatus.Failed => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                BatchTaskStatus.Running => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                BatchTaskStatus.Retrying => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                BatchTaskStatus.Cancelled => new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                _ => new SolidColorBrush(Color.FromRgb(189, 189, 189))
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}

public class PriorityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Critical => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                TaskPriority.High => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                TaskPriority.Normal => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                TaskPriority.Low => new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}

public class BoolToSuccessColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
            : new SolidColorBrush(Color.FromRgb(244, 67, 54));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value != null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}
