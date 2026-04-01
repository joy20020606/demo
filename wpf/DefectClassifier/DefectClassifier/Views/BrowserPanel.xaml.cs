using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DefectClassifier.Models;
using DefectClassifier.ViewModels;

namespace DefectClassifier.Views;

public partial class BrowserPanel : UserControl
{
    private Point _dragStart;
    private bool _isDragging;

    public BrowserPanel()
    {
        InitializeComponent();
    }

    private void TypeTag_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(null);
        _isDragging = false;
    }

    private void TypeTag_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        var pos = e.GetPosition(null);
        var diff = _dragStart - pos;
        if (!_isDragging &&
            (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
             Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
            _isDragging = true;
            if (sender is FrameworkElement elem && elem.DataContext is DefectType type)
            {
                DragDrop.DoDragDrop(elem, type, DragDropEffects.Copy);
                _isDragging = false;
            }
        }
    }

    private void TypeTag_Click(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging) return;
        if (sender is FrameworkElement elem && elem.DataContext is DefectType type)
        {
            if (DataContext is BrowserViewModel vm)
                vm.SetDefectTypeCommand.Execute(type);
        }
    }

    private void ListView_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(DefectType))
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void ListView_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(DefectType)) is DefectType type)
        {
            if (DataContext is BrowserViewModel vm)
                vm.SetDefectTypeCommand.Execute(type);
        }
    }
}
