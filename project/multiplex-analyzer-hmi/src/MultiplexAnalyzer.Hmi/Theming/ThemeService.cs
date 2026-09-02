using System.Windows;

namespace MultiplexAnalyzer.Hmi.Theming;

public static class ThemeService
{
    private const int TokenSlot = 0;

    public static AppTheme Current { get; private set; } = AppTheme.Light;

    public static void Apply(AppTheme theme)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        dictionaries[TokenSlot] = new ResourceDictionary
        {
            Source = new Uri($"Themes/Tokens.{theme}.xaml", UriKind.Relative)
        };
        Current = theme;
    }

    public static AppTheme Toggle()
    {
        Apply(Current == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
        return Current;
    }
}
