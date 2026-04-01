using System.Windows.Controls;
using WaferMapViewer.Controls;
using WaferMapViewer.ViewModels;

namespace WaferMapViewer.Views;

public partial class WaferMapPanel : UserControl
{
    public WaferMapPanel()
    {
        InitializeComponent();
        WaferControl.DieClicked += (s, e) =>
        {
            if (DataContext is WaferMapViewModel vm)
                vm.SelectedDie = e.Die;
        };
    }
}
