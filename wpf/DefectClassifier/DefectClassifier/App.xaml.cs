using System.Windows;
using DefectClassifier.ViewModels;
using DefectClassifier.Views;

namespace DefectClassifier;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var vm = new MainViewModel();
        var window = new MainWindow { DataContext = vm };
        window.Show();
    }
}
