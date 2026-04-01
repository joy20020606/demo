using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RealtimeDataMonitor.Models;

namespace RealtimeDataMonitor.ViewModels;

public partial class ChannelSettingsViewModel : ObservableObject
{
    public ObservableCollection<ChannelConfig> Channels { get; }

    public ChannelSettingsViewModel(List<ChannelConfig> channels)
    {
        Channels = new ObservableCollection<ChannelConfig>(channels);
    }
}
