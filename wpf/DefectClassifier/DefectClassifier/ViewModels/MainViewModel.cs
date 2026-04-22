using CommunityToolkit.Mvvm.ComponentModel;

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

    public MainViewModel(BrowserViewModel browserVM, StatisticsViewModel statisticsVM, ClassificationViewModel classificationVM)
    {
        BrowserVM = browserVM;
        StatisticsVM = statisticsVM;
        ClassificationVM = classificationVM;

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
