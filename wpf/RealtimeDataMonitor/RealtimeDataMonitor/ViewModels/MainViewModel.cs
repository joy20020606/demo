using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RealtimeDataMonitor.Models;
using RealtimeDataMonitor.Services;

namespace RealtimeDataMonitor.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly List<ChannelConfig> _channels;
    private readonly DataAcquisitionService _daq;
    private readonly AlarmService _alarmService;
    private readonly CsvExportService _csvExport;

    public ChartViewModel Chart { get; }
    public AlarmViewModel Alarm { get; }
    public ChannelSettingsViewModel ChannelSettings { get; }

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private double _timeWindowSeconds = 30;

    [ObservableProperty]
    private int _samplesPerSecond = 200;

    [ObservableProperty]
    private bool _isExporting;

    private bool _disposed;

    public MainViewModel()
    {
        _channels = ChannelConfig.CreateDefaults();
        _daq = new DataAcquisitionService(_channels);
        _alarmService = new AlarmService(_channels);
        _csvExport = new CsvExportService();

        Chart = new ChartViewModel(_channels);
        Alarm = new AlarmViewModel();
        ChannelSettings = new ChannelSettingsViewModel(_channels);

        _daq.DataAvailable += OnDataAvailable;
        _alarmService.AlarmTriggered += OnAlarmTriggered;
    }

    private void OnDataAvailable(IReadOnlyList<SensorData> data)
    {
        Chart.AppendData(data);
        _alarmService.CheckData(data);
    }

    private void OnAlarmTriggered(AlarmEvent alarm)
    {
        Alarm.AddAlarm(alarm);
        Application.Current?.Dispatcher.Invoke(() =>
            StatusText = $"ALARM: {alarm.Description} [{alarm.Timestamp:HH:mm:ss}]");
    }

    [RelayCommand]
    private void Start()
    {
        _daq.SamplesPerSecond = SamplesPerSecond;
        _daq.Start();
        IsRunning = true;
        StatusText = "Acquiring...";
    }

    [RelayCommand]
    private void Stop()
    {
        _daq.Stop();
        IsRunning = false;
        StatusText = "Stopped";
    }

    [RelayCommand]
    private void ClearChart()
    {
        Chart.Clear();
        StatusText = "Chart cleared";
    }

    partial void OnTimeWindowSecondsChanged(double value)
    {
        Chart.TimeWindowSeconds = value;
    }

    [RelayCommand]
    private async Task ExportDataAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"sensor_data_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dlg.ShowDialog() != true) return;

        IsExporting = true;
        StatusText = "Exporting...";

        try
        {
            var history = Chart.GetAllHistory();
            await _csvExport.ExportAsync(history, _channels, dlg.FileName);
            StatusText = $"Exported {history.Count} records to {System.IO.Path.GetFileName(dlg.FileName)}";
        }
        catch (Exception ex)
        {
            StatusText = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private async Task ExportAlarmsAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"alarms_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dlg.ShowDialog() != true) return;

        IsExporting = true;
        StatusText = "Exporting alarms...";

        try
        {
            await _csvExport.ExportAlarmsAsync(Alarm.Alarms, dlg.FileName);
            StatusText = $"Exported {Alarm.Alarms.Count} alarms to {System.IO.Path.GetFileName(dlg.FileName)}";
        }
        catch (Exception ex)
        {
            StatusText = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _daq.DataAvailable -= OnDataAvailable;
        _alarmService.AlarmTriggered -= OnAlarmTriggered;
        _daq.Dispose();
        GC.SuppressFinalize(this);
    }
}
