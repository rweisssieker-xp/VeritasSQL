namespace VeritasSQL.Core.Models;

/// <summary>
/// Application settings
/// </summary>
public class AppSettings
{
    public string? EncryptedOpenAIApiKey { get; set; }
    public int DefaultRowLimit { get; set; } = 100;
    public int MaxRowLimit { get; set; } = 10000;
    public int QueryTimeoutSeconds { get; set; } = 30;
    public string Language { get; set; } = "en-US";
    public bool DryRunByDefault { get; set; } = true;
    public bool ShowExplanations { get; set; } = true;
    public string OpenAIModel { get; set; } = "gpt-5-nano";
    
    public string? GetOpenAIApiKey()
    {
        if (string.IsNullOrEmpty(EncryptedOpenAIApiKey))
            return null;

        try
        {
            var encryptedBytes = Convert.FromBase64String(EncryptedOpenAIApiKey);
            var decryptedBytes = System.Security.Cryptography.ProtectedData.Unprotect(
                encryptedBytes, 
                null, 
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return null;
        }
    }

    public void SetOpenAIApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            EncryptedOpenAIApiKey = null;
            return;
        }

        var plainBytes = System.Text.Encoding.UTF8.GetBytes(apiKey);
        var encryptedBytes = System.Security.Cryptography.ProtectedData.Protect(
            plainBytes, 
            null, 
            System.Security.Cryptography.DataProtectionScope.CurrentUser);
        EncryptedOpenAIApiKey = Convert.ToBase64String(encryptedBytes);
    }
}

