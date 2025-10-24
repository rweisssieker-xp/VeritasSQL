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

        // Global exception handler for unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            MessageBox.Show($"Unhandled Exception: {ex?.Message}\n\nStack Trace:\n{ex?.StackTrace}",
                "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        this.DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show($"UI Thread Exception: {args.Exception.Message}\n\nStack Trace:\n{args.Exception.StackTrace}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        try
        {
            // Dependency Injection Setup
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Hauptfenster anzeigen
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup Error: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
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

        // OpenAI Service - create with empty key initially, will be configured later
        services.AddSingleton<OpenAIService>(sp =>
        {
            // Start with empty API key to avoid blocking on startup
            // Using gpt-4o - the latest and most capable model
            return new OpenAIService("", "gpt-4o");
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
