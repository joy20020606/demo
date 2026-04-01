using System.Collections.Specialized;
using System.Windows.Controls;

namespace SocketCommSimulator.Views;

public partial class LogPanel : UserControl
{
    public LogPanel()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => SubscribeToLog();
    }

    private void SubscribeToLog()
    {
        if (DataContext is ViewModels.LogViewModel vm)
        {
            vm.LogLines.CollectionChanged += LogLines_CollectionChanged;
        }
    }

    private void LogLines_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            LogListBox.ScrollIntoView(LogListBox.Items[^1]);
    }
}
