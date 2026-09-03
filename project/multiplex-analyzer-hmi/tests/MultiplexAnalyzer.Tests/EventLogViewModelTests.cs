using MultiplexAnalyzer.Hmi.Services;
using MultiplexAnalyzer.Hmi.ViewModels;

namespace MultiplexAnalyzer.Tests;

public sealed class EventLogViewModelTests
{
    private readonly EventLogViewModel vm = new(new FakeLogService(seed: 3));

    [Fact]
    public void Starts_unfiltered()
    {
        Assert.Equal(5000, vm.TotalCount);
        Assert.Equal(5000, vm.VisibleCount);
    }

    [Fact]
    public void Level_filter_narrows_the_view()
    {
        vm.SelectedLevel = "Error";

        Assert.True(vm.VisibleCount > 0);
        Assert.True(vm.VisibleCount < vm.TotalCount);
    }

    [Fact]
    public void Search_is_case_insensitive_and_clear_restores_everything()
    {
        vm.SearchText = "WASH";
        var matched = vm.VisibleCount;
        Assert.True(matched > 0);
        Assert.True(matched < vm.TotalCount);

        vm.ClearFiltersCommand.Execute(null);

        Assert.Equal("All", vm.SelectedLevel);
        Assert.Equal(string.Empty, vm.SearchText);
        Assert.Equal(vm.TotalCount, vm.VisibleCount);
    }

    [Fact]
    public void Filter_changes_request_a_measurement()
    {
        var requests = 0;
        vm.MeasurementRequested += (_, _) => requests++;

        vm.SelectedLevel = "Warning";
        vm.IsVirtualizationEnabled = false;

        Assert.Equal(2, requests);
    }
}
