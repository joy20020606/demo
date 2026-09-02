namespace MultiplexAnalyzer.Hmi.ViewModels;

public sealed class NavItemViewModel
{
    public NavItemViewModel(string label, string iconKey, PageViewModelBase page)
    {
        Label = label;
        IconKey = iconKey;
        Page = page;
    }

    public string Label { get; }

    public string IconKey { get; }

    public PageViewModelBase Page { get; }
}
