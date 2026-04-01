using System.Windows;
using RecipeEditor.ViewModels;

namespace RecipeEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
