using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using DataVision.Desktop.Services;
using DataVision.Desktop.ViewModels;
using DataVision.Desktop.Views;

namespace DataVision.Desktop;

public partial class App : Application
{
    public static Window MainWindow { get; set; } = null!;
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();

        UnhandledException += (s, e) =>
        {
            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DataVisionAI");
                Directory.CreateDirectory(folder);
                File.AppendAllText(Path.Combine(folder, "error.log"), $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {e.Message}\n{e.Exception}\n");
            }
            catch { }
        };
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<ExcelReaderService>();
        services.AddSingleton<AiAnalysisService>();
        services.AddSingleton<ChartGeneratorService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<AiAnalystViewModel>();

        // Views
        services.AddTransient<DashboardPage>();
        services.AddTransient<ChartsPage>();
        services.AddTransient<AiAnalystPage>();
        services.AddTransient<DataPreviewPage>();
        services.AddTransient<SettingsPage>();
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = Services.GetRequiredService<MainWindow>();
        MainWindow.Activate();
    }
}
