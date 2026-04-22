using System.IO;
using System.Windows;
using DefectClassifier.Services;
using DefectClassifier.Services.ClassificationStrategy;
using DefectClassifier.ViewModels;
using DefectClassifier.Views;

namespace DefectClassifier;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

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

        var browserVM = new BrowserViewModel(repository, strategies);
        var statisticsVM = new StatisticsViewModel(repository, statisticsService);
        var classificationVM = new ClassificationViewModel(repository, strategies, browserVM);

        var mainVM = new MainViewModel(browserVM, statisticsVM, classificationVM);
        new MainWindow { DataContext = mainVM }.Show();
    }
}
