using System.Windows;
using System.Windows.Input;
using ImageInspector.Models;
using ImageInspector.ViewModels;

namespace ImageInspector.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void MenuItem_Exit(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Rectangle_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.SelectedShape = AnnotationShape.Rectangle;
    }

    private void Ellipse_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.SelectedShape = AnnotationShape.Ellipse;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Delete && DataContext is MainViewModel vm)
        {
            vm.DeleteSelectedAnnotationCommand.Execute(null);
        }
    }
}
