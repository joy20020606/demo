using CommunityToolkit.Mvvm.ComponentModel;
using PluginArchitecture.Core;

namespace PluginArchitecture.App.Models;

public partial class PluginItemModel : ObservableObject
{
    private readonly IAnalysisPlugin _plugin;

    public PluginItemModel(IAnalysisPlugin plugin)
    {
        _plugin = plugin;
        _isEnabled = plugin.IsEnabled;
    }

    public IAnalysisPlugin Plugin => _plugin;
    public string Name => _plugin.Name;
    public string Description => _plugin.Description;
    public string Version => _plugin.Version;

    [ObservableProperty]
    private bool _isEnabled;

    partial void OnIsEnabledChanged(bool value)
    {
        _plugin.IsEnabled = value;
    }
}
