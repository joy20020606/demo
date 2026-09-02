using System.Windows;
using MultiplexAnalyzer.Hmi.Theming;

namespace MultiplexAnalyzer.Hmi;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        ThemeLabel.Text = ThemeService.Toggle().ToString();
    }
}
