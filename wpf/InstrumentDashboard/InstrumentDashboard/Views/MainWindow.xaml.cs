using System.Windows;
using InstrumentDashboard.ViewModels;

namespace InstrumentDashboard.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
