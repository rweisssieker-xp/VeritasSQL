using System.Text.Json;
using VeritasSQL.Core.Models;

namespace VeritasSQL.Core.Services;

/// <summary>
/// Manages database connection profiles
/// </summary>
public class ConnectionManager
{
    private readonly string _configPath;
    private List<ConnectionProfile> _profiles = new();

    public ConnectionManager()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VeritasSQL");
        
        Directory.CreateDirectory(appDataPath);
        _configPath = Path.Combine(appDataPath, "connections.json");
    }

    public async Task<List<ConnectionProfile>> GetProfilesAsync()
    {
        if (_profiles.Count == 0)
        {
            await LoadProfilesAsync();
        }
        return _profiles;
    }

    public async Task<ConnectionProfile?> GetProfileByIdAsync(string id)
    {
        var profiles = await GetProfilesAsync();
        return profiles.FirstOrDefault(p => p.Id == id);
    }

    public async Task SaveProfileAsync(ConnectionProfile profile)
    {
        var profiles = await GetProfilesAsync();
        
        var existing = profiles.FirstOrDefault(p => p.Id == profile.Id);
        if (existing != null)
        {
            profiles.Remove(existing);
        }
        
        profiles.Add(profile);
        _profiles = profiles;
        
        await SaveProfilesAsync();
    }

    public async Task DeleteProfileAsync(string id)
    {
        var profiles = await GetProfilesAsync();
        profiles.RemoveAll(p => p.Id == id);
        _profiles = profiles;
        await SaveProfilesAsync();
    }

    public async Task<bool> TestConnectionAsync(ConnectionProfile profile)
    {
        try
        {
            var connectionString = profile.GetConnectionString();
            await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task LoadProfilesAsync()
    {
        if (!File.Exists(_configPath))
        {
            _profiles = new List<ConnectionProfile>();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_configPath);
            _profiles = JsonSerializer.Deserialize<List<ConnectionProfile>>(json) ?? new();
        }
        catch
        {
            _profiles = new List<ConnectionProfile>();
        }
    }

    private async Task SaveProfilesAsync()
    {
        var json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(_configPath, json);
    }
}

