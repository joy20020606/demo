using System.Globalization;
using System.Windows.Data;
using DefectClassifier.Models;
using DefectClassifier.ViewModels;

namespace DefectClassifier.Converters;

public class DefectTypeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DefectType t ? BrowserViewModel.GetTypeName(t) : value?.ToString() ?? string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
