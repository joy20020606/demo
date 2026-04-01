using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PluginArchitecture.App.Services;
using PluginArchitecture.App.ViewModels;
using PluginArchitecture.App.Views;
using PluginArchitecture.Core;
using PluginArchitecture.Plugins;

namespace PluginArchitecture.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var window = _serviceProvider.GetRequiredService<MainWindow>();
        window.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ImageProcessingService>();

        services.AddSingleton<IAnalysisPlugin, EdgeDetectionPlugin>();
        services.AddSingleton<IAnalysisPlugin, BinarizationPlugin>();
        services.AddSingleton<IAnalysisPlugin, NoiseReductionPlugin>();

        services.AddSingleton<PluginLoader>(sp =>
        {
            var loader = new PluginLoader();
            var plugins = sp.GetServices<IAnalysisPlugin>();
            loader.LoadBuiltInPlugins(plugins);
            return loader;
        });

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
