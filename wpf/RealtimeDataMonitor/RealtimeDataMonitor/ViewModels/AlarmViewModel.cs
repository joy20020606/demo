using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RealtimeDataMonitor.Models;

namespace RealtimeDataMonitor.ViewModels;

public partial class AlarmViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<AlarmEvent> _alarms = [];

    [ObservableProperty]
    private int _activeAlarmCount;

    [ObservableProperty]
    private int _criticalAlarmCount;

    public void AddAlarm(AlarmEvent alarm)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            Alarms.Insert(0, alarm);
            if (Alarms.Count > 200)
                Alarms.RemoveAt(Alarms.Count - 1);

            ActiveAlarmCount = Alarms.Count(a => !a.IsAcknowledged);
            CriticalAlarmCount = Alarms.Count(a => a.Severity == AlarmSeverity.Critical && !a.IsAcknowledged);
        });
    }

    [RelayCommand]
    private void AcknowledgeAll()
    {
        foreach (var a in Alarms)
            a.IsAcknowledged = true;

        ActiveAlarmCount = 0;
        CriticalAlarmCount = 0;
    }

    [RelayCommand]
    private void ClearAll()
    {
        Alarms.Clear();
        ActiveAlarmCount = 0;
        CriticalAlarmCount = 0;
    }
}
