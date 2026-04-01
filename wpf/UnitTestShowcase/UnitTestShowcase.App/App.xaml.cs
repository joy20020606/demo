using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using UnitTestShowcase.App.Services;
using UnitTestShowcase.App.ViewModels;
using UnitTestShowcase.App.Views;

namespace UnitTestShowcase.App;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UnitTestShowcase", "products.db");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);
        var connectionString = $"Data Source={dbPath}";

        services.AddSingleton<IProductRepository>(_ => new ProductRepository(connectionString));
        services.AddSingleton<IProductService, ProductService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
