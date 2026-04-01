using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RecipeEditor.ViewModels;

namespace RecipeEditor.Views;

public partial class StepEditor : UserControl
{
    private Point _startPoint;
    private bool _isDragging;

    public StepEditor()
    {
        InitializeComponent();
    }

    private void StepList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(null);
        _isDragging = false;
    }

    private void StepList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _isDragging) return;

        var pos = e.GetPosition(null);
        var diff = _startPoint - pos;
        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance) return;

        var item = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        if (item?.DataContext is StepViewModel stepVm)
        {
            _isDragging = true;
            var data = new DataObject("StepViewModel", stepVm);
            DragDrop.DoDragDrop(item, data, DragDropEffects.Move);
            _isDragging = false;
        }
    }

    private void StepList_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent("StepViewModel")
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void StepList_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("StepViewModel")) return;
        if (e.Data.GetData("StepViewModel") is not StepViewModel source) return;

        var target = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        if (target?.DataContext is StepViewModel targetStep && targetStep != source)
        {
            if (DataContext is RecipeViewModel vm)
                vm.MoveStep(source, targetStep);
        }
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T t) return t;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
