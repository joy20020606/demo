using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SocketCommSimulator.Models;

namespace SocketCommSimulator.ViewModels;

public partial class LogViewModel : ObservableObject
{
    private readonly object _lock = new();

    [ObservableProperty]
    private DisplayFormat _displayFormat = DisplayFormat.Ascii;

    [ObservableProperty]
    private ObservableCollection<string> _logLines = [];

    private readonly List<LogEntry> _entries = [];

    public void AddEntry(LogEntry entry)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }

        Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            LogLines.Add(entry.Format(DisplayFormat));
            if (LogLines.Count > 1000)
                LogLines.RemoveAt(0);
        });
    }

    partial void OnDisplayFormatChanged(DisplayFormat value)
    {
        RefreshLines();
    }

    private void RefreshLines()
    {
        List<LogEntry> snapshot;
        lock (_lock) snapshot = [.._entries];

        Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            LogLines.Clear();
            foreach (var entry in snapshot.TakeLast(1000))
                LogLines.Add(entry.Format(DisplayFormat));
        });
    }

    [RelayCommand]
    private void Clear()
    {
        lock (_lock) _entries.Clear();
        Application.Current?.Dispatcher.InvokeAsync(() => LogLines.Clear());
    }

    [RelayCommand]
    private void ToggleFormat()
    {
        DisplayFormat = DisplayFormat == DisplayFormat.Ascii ? DisplayFormat.Hex : DisplayFormat.Ascii;
    }
}
