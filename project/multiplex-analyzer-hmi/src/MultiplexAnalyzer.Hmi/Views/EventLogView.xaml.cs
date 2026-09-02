using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MultiplexAnalyzer.Hmi.ViewModels;

namespace MultiplexAnalyzer.Hmi.Views;

public partial class EventLogView : UserControl
{
    private EventLogViewModel? viewModel;

    public EventLogView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += (_, _) => viewModel?.RequestMeasurement();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (viewModel is not null)
        {
            viewModel.MeasurementRequested -= OnMeasurementRequested;
        }

        viewModel = e.NewValue as EventLogViewModel;

        if (viewModel is not null)
        {
            viewModel.MeasurementRequested += OnMeasurementRequested;
        }
    }

    private void OnMeasurementRequested(object? sender, EventArgs e)
    {
        var stopwatch = Stopwatch.StartNew();

        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, () =>
        {
            stopwatch.Stop();
            var memoryMb = Process.GetCurrentProcess().PrivateMemorySize64 / (1024d * 1024d);
            viewModel?.ReportMeasurement(stopwatch.Elapsed.TotalMilliseconds, CountRealizedContainers(), memoryMb);
        });
    }

    private int CountRealizedContainers()
    {
        var generator = LogList.ItemContainerGenerator;
        var realized = 0;

        for (var index = 0; index < LogList.Items.Count; index++)
        {
            if (generator.ContainerFromIndex(index) is not null)
            {
                realized++;
            }
        }

        return realized;
    }
}
