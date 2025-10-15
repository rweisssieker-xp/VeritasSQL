using System.Text.Json;
using VeritasSQL.Core.Models;

namespace VeritasSQL.Core.Services;

/// <summary>
/// Manages application settings
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings? _settings;

    public SettingsService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VeritasSQL");
        
        Directory.CreateDirectory(appDataPath);
        _settingsPath = Path.Combine(appDataPath, "settings.json");
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        if (_settings == null)
        {
            await LoadSettingsAsync();
        }
        return _settings!;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _settings = settings;
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(_settingsPath, json);
    }

    private async Task LoadSettingsAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            _settings = new AppSettings();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsPath);
            _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new();
        }
        catch
        {
            _settings = new AppSettings();
        }
    }
}

