using System.Windows;
using BatchProcessManager.ViewModels;

namespace BatchProcessManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
