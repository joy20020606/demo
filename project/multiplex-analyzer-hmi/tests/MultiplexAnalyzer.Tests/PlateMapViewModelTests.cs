using MultiplexAnalyzer.Hmi.Services;
using MultiplexAnalyzer.Hmi.ViewModels;

namespace MultiplexAnalyzer.Tests;

public sealed class PlateMapViewModelTests
{
    private readonly PlateMapViewModel vm = new(new FakePlateService(seed: 1));

    [Fact]
    public void Loads_a_full_96_well_plate()
    {
        Assert.Equal(96, vm.Wells.Count);
        Assert.Equal("A1", vm.Wells[0].Label);
        Assert.Equal("H12", vm.Wells[^1].Label);
        Assert.All(vm.Wells, well => Assert.Equal(8, well.Markers.Count));
    }

    [Fact]
    public void Zoom_is_clamped_and_commands_follow_the_bounds()
    {
        Assert.False(vm.ZoomOutCommand.CanExecute(null));

        for (var i = 0; i < 20; i++)
        {
            vm.ZoomInCommand.Execute(null);
        }

        Assert.Equal(6, vm.Zoom);
        Assert.False(vm.ZoomInCommand.CanExecute(null));
        Assert.True(vm.ZoomOutCommand.CanExecute(null));
    }

    [Fact]
    public void Reset_view_clears_zoom_and_offsets()
    {
        vm.ZoomInCommand.Execute(null);
        vm.OffsetX = -40;
        vm.OffsetY = -12;
        Assert.True(vm.ResetViewCommand.CanExecute(null));

        vm.ResetViewCommand.Execute(null);

        Assert.Equal(1, vm.Zoom);
        Assert.Equal(0, vm.OffsetX);
        Assert.Equal(0, vm.OffsetY);
        Assert.False(vm.ResetViewCommand.CanExecute(null));
    }

    [Fact]
    public void Selecting_a_well_exposes_its_markers()
    {
        var target = vm.Wells.First(well => well.PositiveCount > 0);

        vm.SelectedWell = target;

        Assert.True(vm.HasSelection);
        Assert.Contains(vm.SelectedWell!.Markers, marker => marker.IsPositive);

        vm.ClearSelectionCommand.Execute(null);
        Assert.False(vm.HasSelection);
    }
}
