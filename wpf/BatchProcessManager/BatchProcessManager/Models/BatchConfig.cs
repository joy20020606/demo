using CommunityToolkit.Mvvm.ComponentModel;

namespace BatchProcessManager.Models;

public partial class BatchConfig : ObservableObject
{
    [ObservableProperty] private int _maxConcurrency = 3;
    [ObservableProperty] private int _maxRetries = 3;
    [ObservableProperty] private int _timeoutSeconds = 30;
    [ObservableProperty] private bool _stopOnFirstFailure = false;
    [ObservableProperty] private int _retryDelaySeconds = 2;
}
