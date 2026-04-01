using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using DefectClassifier.Services;
using DefectClassifier.Services.ClassificationStrategy;

namespace DefectClassifier.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public BrowserViewModel BrowserVM { get; }
    public StatisticsViewModel StatisticsVM { get; }
    public ClassificationViewModel ClassificationVM { get; }

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private string _statusText = "就緒";

    public MainViewModel()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DefectClassifier", "defects.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        var repository = new DefectRepository(dbPath);
        repository.SeedSampleData();

        var statisticsService = new StatisticsService();
        var strategies = new List<IClassificationStrategy>
        {
            new SizeBasedStrategy(),
            new LocationBasedStrategy()
        };

        BrowserVM = new BrowserViewModel(repository, strategies);
        StatisticsVM = new StatisticsViewModel(repository, statisticsService);
        ClassificationVM = new ClassificationViewModel(repository, strategies, BrowserVM);

        BrowserVM.StatusChanged += msg => StatusText = msg;
        StatisticsVM.StatusChanged += msg => StatusText = msg;
        ClassificationVM.StatusChanged += msg => StatusText = msg;

        StatisticsVM.Refresh();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        if (value == 1) StatisticsVM.Refresh();
        if (value == 2) ClassificationVM.Refresh();
    }
}
