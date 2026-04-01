using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InstrumentDashboard.ViewModels;

public partial class LogEntry : ObservableObject
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string Level { get; init; } = "INFO";
    public string Message { get; init; } = string.Empty;

    public string Display => $"[{Timestamp:HH:mm:ss}] [{Level}] {Message}";
}

public partial class LogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _filterText = string.Empty;

    public ObservableCollection<LogEntry> Entries { get; } = new();

    public void AddEntry(string message, string level = "INFO")
    {
        App.Current.Dispatcher.Invoke(() =>
            Entries.Add(new LogEntry { Timestamp = DateTime.Now, Level = level, Message = message }));
    }

    [RelayCommand]
    private void Clear() => Entries.Clear();

    [RelayCommand]
    private void Export()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            FileName = $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        };

        if (dialog.ShowDialog() != true) return;

        var lines = Entries.Select(e => e.Display);
        File.WriteAllLines(dialog.FileName, lines);
    }
}
