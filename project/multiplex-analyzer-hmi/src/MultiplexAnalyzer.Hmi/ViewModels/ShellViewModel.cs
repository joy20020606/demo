using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiplexAnalyzer.Hmi.Theming;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private NavItemViewModel selectedNav;

    [ObservableProperty]
    private AppTheme theme = ThemeService.Current;

    public ShellViewModel()
    {
        NavItems = new ObservableCollection<NavItemViewModel>
        {
            new("Dashboard", "Icon.Dashboard", new DashboardViewModel()),
            new("Plate", "Icon.PlateMap", new PlateMapViewModel()),
            new("Settings", "Icon.Settings", new SettingsViewModel()),
            new("Log", "Icon.EventLog", new EventLogViewModel())
        };

        selectedNav = NavItems[0];
    }

    public ObservableCollection<NavItemViewModel> NavItems { get; }

    public PageViewModelBase CurrentPage => SelectedNav.Page;

    [RelayCommand]
    private void ToggleTheme() => Theme = ThemeService.Toggle();

    partial void OnSelectedNavChanged(NavItemViewModel value) => OnPropertyChanged(nameof(CurrentPage));
}
