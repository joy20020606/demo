using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WaferMapViewer.Services;

namespace WaferMapViewer.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public WaferMapViewModel WaferMapVM { get; } = new();
    public DieDetailViewModel DieDetailVM { get; } = new();
    public StatisticsViewModel StatisticsVM { get; } = new();

    [ObservableProperty] private bool _isLoading;

    public MainViewModel()
    {
        WaferMapVM.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(WaferMapViewModel.SelectedDie) && WaferMapVM.SelectedDie != null)
                DieDetailVM.Select(WaferMapVM.SelectedDie);
        };
        GenerateWafer();
    }

    [RelayCommand]
    private void GenerateWafer()
    {
        IsLoading = true;
        var wafer = WaferDataGenerator.Generate();
        WaferMapVM.LoadWafer(wafer);
        StatisticsVM.Update(wafer);
        DieDetailVM.SelectedDie = null;
        IsLoading = false;
    }
}
