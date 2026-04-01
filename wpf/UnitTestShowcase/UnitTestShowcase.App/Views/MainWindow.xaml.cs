using System.Windows;
using UnitTestShowcase.App.ViewModels;

namespace UnitTestShowcase.App.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadProductsAsync();
    }
}
