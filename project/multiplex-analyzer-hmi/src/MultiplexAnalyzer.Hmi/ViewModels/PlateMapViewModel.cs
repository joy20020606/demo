using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiplexAnalyzer.Hmi.Services;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public sealed partial class PlateMapViewModel : PageViewModelBase
{
    private const double ZoomStep = 1.25;
    private const double MinZoom = 1;
    private const double MaxZoom = 6;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    private WellViewModel? selectedWell;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ZoomInCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomOutCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetViewCommand))]
    [NotifyPropertyChangedFor(nameof(ZoomLabel))]
    private double zoom = 1;

    [ObservableProperty]
    private double offsetX;

    [ObservableProperty]
    private double offsetY;

    public PlateMapViewModel(IPlateService plateService)
    {
        Rows = plateService.Rows;
        Columns = plateService.Columns;
        Wells = plateService.LoadPlate().Select(well => new WellViewModel(well)).ToList();
        PositiveWellCount = Wells.Count(well => well.PositiveCount > 0);
    }

    public override string Title => "Plate Map";

    public override string Subtitle => "96 wells drawn in one OnRender pass. Pinch or scroll to zoom, drag to pan, tap a well for markers.";

    public int Rows { get; }

    public int Columns { get; }

    public IReadOnlyList<WellViewModel> Wells { get; }

    public int PositiveWellCount { get; }

    public bool HasSelection => SelectedWell is not null;

    public string ZoomLabel => $"{Zoom * 100:0}%";

    [RelayCommand(CanExecute = nameof(CanZoomIn))]
    private void ZoomIn() => Zoom = Math.Min(Zoom * ZoomStep, MaxZoom);

    private bool CanZoomIn() => Zoom < MaxZoom;

    [RelayCommand(CanExecute = nameof(CanZoomOut))]
    private void ZoomOut() => Zoom = Math.Max(Zoom / ZoomStep, MinZoom);

    private bool CanZoomOut() => Zoom > MinZoom;

    [RelayCommand(CanExecute = nameof(CanResetView))]
    private void ResetView()
    {
        Zoom = MinZoom;
        OffsetX = 0;
        OffsetY = 0;
    }

    private bool CanResetView() => Zoom > MinZoom || OffsetX != 0 || OffsetY != 0;

    [RelayCommand]
    private void ClearSelection() => SelectedWell = null;

    partial void OnOffsetXChanged(double value) => ResetViewCommand.NotifyCanExecuteChanged();

    partial void OnOffsetYChanged(double value) => ResetViewCommand.NotifyCanExecuteChanged();
}
