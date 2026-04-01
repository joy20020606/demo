using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PluginArchitecture.App.Models;
using PluginArchitecture.Core;

namespace PluginArchitecture.App.ViewModels;

public partial class PluginDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private PluginItemModel? _selectedPlugin;

    public ObservableCollection<PluginParameter> CurrentParameters { get; } = new();

    partial void OnSelectedPluginChanged(PluginItemModel? value)
    {
        CurrentParameters.Clear();
        if (value == null) return;
        foreach (var param in value.Plugin.Parameters)
            CurrentParameters.Add(param);
    }

    [RelayCommand]
    private void ResetParameters()
    {
        if (SelectedPlugin == null) return;
        foreach (var param in SelectedPlugin.Plugin.Parameters)
        {
            param.Value = param.DefaultValue;
            SelectedPlugin.Plugin.SetParameter(param.Key, param.DefaultValue);
        }
        OnPropertyChanged(nameof(CurrentParameters));
        var temp = SelectedPlugin;
        SelectedPlugin = null;
        SelectedPlugin = temp;
    }
}
