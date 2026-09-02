using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiplexAnalyzer.Hmi.Services;
using MultiplexAnalyzer.Hmi.Theming;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private NavItemViewModel selectedNav;

    [ObservableProperty]
    private AppTheme theme = ThemeService.Current;

    public ShellViewModel()
        : this(new FakeDeviceService())
    {
    }

    public ShellViewModel(IDeviceService device)
    {
        Dashboard = new DashboardViewModel(device);

        NavItems =
        [
            new NavItemViewModel("Dashboard", "Icon.Dashboard", Dashboard),
            new NavItemViewModel("Plate", "Icon.PlateMap", new PlateMapViewModel()),
            new NavItemViewModel("Settings", "Icon.Settings", new SettingsViewModel()),
            new NavItemViewModel("Log", "Icon.EventLog", new EventLogViewModel())
        ];

        selectedNav = NavItems[0];
    }

    public ObservableCollection<NavItemViewModel> NavItems { get; }

    public DashboardViewModel Dashboard { get; }

    public PageViewModelBase CurrentPage => SelectedNav.Page;

    [RelayCommand]
    private void ToggleTheme() => Theme = ThemeService.Toggle();

    partial void OnSelectedNavChanged(NavItemViewModel value) => OnPropertyChanged(nameof(CurrentPage));
}
