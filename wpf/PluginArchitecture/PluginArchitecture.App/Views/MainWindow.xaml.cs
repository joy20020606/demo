using System.Windows;
using PluginArchitecture.App.ViewModels;

namespace PluginArchitecture.App.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
