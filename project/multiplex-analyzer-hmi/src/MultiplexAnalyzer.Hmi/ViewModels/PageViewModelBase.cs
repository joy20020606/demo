using CommunityToolkit.Mvvm.ComponentModel;

namespace MultiplexAnalyzer.Hmi.ViewModels;

public abstract class PageViewModelBase : ObservableObject
{
    public abstract string Title { get; }

    public abstract string Subtitle { get; }
}
