using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PluginArchitecture.App.Models;
using PluginArchitecture.App.Services;
using PluginArchitecture.Core;

namespace PluginArchitecture.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ImageProcessingService _imageService;
    private readonly PluginLoader _pluginLoader;

    public MainViewModel(ImageProcessingService imageService, PluginLoader pluginLoader)
    {
        _imageService = imageService;
        _pluginLoader = pluginLoader;

        foreach (var plugin in pluginLoader.LoadedPlugins)
            Plugins.Add(new PluginItemModel(plugin));

        OriginalImage = _imageService.GenerateSampleImage(400, 300);
        ProcessedImage = OriginalImage;
    }

    public ObservableCollection<PluginItemModel> Plugins { get; } = new();

    [ObservableProperty]
    private PluginItemModel? _selectedPlugin;

    [ObservableProperty]
    private BitmapSource? _originalImage;

    [ObservableProperty]
    private BitmapSource? _processedImage;

    [ObservableProperty]
    private string _statusMessage = "就緒";

    [RelayCommand]
    private void ApplyPlugin()
    {
        if (SelectedPlugin == null || OriginalImage == null)
        {
            StatusMessage = "請先選擇外掛";
            return;
        }

        if (!SelectedPlugin.IsEnabled)
        {
            StatusMessage = $"{SelectedPlugin.Name} 已停用";
            return;
        }

        ProcessedImage = _imageService.ApplyPlugin(OriginalImage, SelectedPlugin.Plugin);
        StatusMessage = $"已套用外掛：{SelectedPlugin.Name}";
    }

    [RelayCommand]
    private void ApplyAllEnabled()
    {
        if (OriginalImage == null) return;

        var enabledPlugins = Plugins.Where(p => p.IsEnabled).Select(p => p.Plugin).ToList();
        if (enabledPlugins.Count == 0)
        {
            StatusMessage = "沒有已啟用的外掛";
            return;
        }

        var current = OriginalImage;
        foreach (var plugin in enabledPlugins)
            current = _imageService.ApplyPlugin(current, plugin);

        ProcessedImage = current;
        StatusMessage = $"已依序套用 {enabledPlugins.Count} 個外掛";
    }

    [RelayCommand]
    private void ResetImage()
    {
        ProcessedImage = OriginalImage;
        StatusMessage = "已重置影像";
    }

    [RelayCommand]
    private void GenerateNewSample()
    {
        OriginalImage = _imageService.GenerateSampleImage(400, 300);
        ProcessedImage = OriginalImage;
        StatusMessage = "已產生新範例影像";
    }
}
