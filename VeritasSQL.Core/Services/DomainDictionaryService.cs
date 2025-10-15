using System.Text.Json;

namespace VeritasSQL.Core.Services;

/// <summary>
/// Manages domain dictionary (synonyms/technical terms)
/// </summary>
public class DomainDictionaryService
{
    private readonly string _dictionaryPath;
    private Dictionary<string, string> _dictionary = new();

    public DomainDictionaryService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VeritasSQL");
        
        Directory.CreateDirectory(appDataPath);
        _dictionaryPath = Path.Combine(appDataPath, "domain_dictionary.json");

        _ = LoadDictionaryAsync();
    }

    public async Task<Dictionary<string, string>> GetDictionaryAsync()
    {
        if (_dictionary.Count == 0)
        {
            await LoadDictionaryAsync();
        }
        return new Dictionary<string, string>(_dictionary);
    }

    public async Task SaveDictionaryAsync(Dictionary<string, string> dictionary)
    {
        _dictionary = new Dictionary<string, string>(dictionary);
        var json = JsonSerializer.Serialize(_dictionary, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(_dictionaryPath, json);
    }

    public async Task AddEntryAsync(string synonym, string actualName)
    {
        _dictionary[synonym.ToLowerInvariant()] = actualName;
        await SaveDictionaryAsync(_dictionary);
    }

    public async Task RemoveEntryAsync(string synonym)
    {
        _dictionary.Remove(synonym.ToLowerInvariant());
        await SaveDictionaryAsync(_dictionary);
    }

    private async Task LoadDictionaryAsync()
    {
        if (!File.Exists(_dictionaryPath))
        {
            // Create standard examples
            _dictionary = new Dictionary<string, string>
            {
                { "customer", "Customers" },
                { "customers", "Customers" },
                { "product", "Products" },
                { "products", "Products" },
                { "order", "Orders" },
                { "orders", "Orders" },
                { "item", "Products" }
            };
            await SaveDictionaryAsync(_dictionary);
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_dictionaryPath);
            _dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json) 
                ?? new Dictionary<string, string>();
        }
        catch
        {
            _dictionary = new Dictionary<string, string>();
        }
    }
}

