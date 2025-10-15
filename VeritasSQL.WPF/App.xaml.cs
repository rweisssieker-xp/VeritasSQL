using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VeritasSQL.Core.Data;
using VeritasSQL.Core.Services;
using VeritasSQL.WPF.ViewModels;

namespace VeritasSQL.WPF;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Dependency Injection Setup
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Hauptfenster anzeigen
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<ConnectionManager>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<SchemaService>();
        services.AddSingleton<QueryExecutor>();
        services.AddSingleton<HistoryService>();
        services.AddSingleton<AuditLogger>();
        services.AddSingleton<DomainDictionaryService>();

        // OpenAI Service wird lazy erstellt (API Key aus Settings)
        services.AddTransient<OpenAIService>(sp =>
        {
            var settingsService = sp.GetRequiredService<SettingsService>();
            var settings = settingsService.GetSettingsAsync().Result;
            return new OpenAIService(settings.GetOpenAIApiKey(), settings.OpenAIModel);
        });

        // Export Services
        services.AddTransient<Core.Export.CsvExporter>();
        services.AddTransient<Core.Export.ExcelExporter>();

        // ViewModels
        services.AddSingleton<MainViewModel>();

        // Windows
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
