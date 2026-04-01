using System.Windows;
using WaferMapViewer.ViewModels;

namespace WaferMapViewer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
