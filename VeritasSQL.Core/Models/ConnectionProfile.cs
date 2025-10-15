namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents a database connection profile
/// </summary>
public class ConnectionProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public AuthenticationType AuthType { get; set; } = AuthenticationType.Windows;
    public string? Username { get; set; }
    public string? EncryptedPassword { get; set; }
    public int ConnectionTimeout { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastUsed { get; set; }

    /// <summary>
    /// Creates a connection string for this profile
    /// </summary>
    public string GetConnectionString()
    {
        var builder = new System.Text.StringBuilder();

        switch (DatabaseType)
        {
            case DatabaseType.SqlServer:
                builder.Append($"Server={Server};");
                builder.Append($"Database={Database};");
                
                if (AuthType == AuthenticationType.Windows)
                {
                    builder.Append("Integrated Security=true;");
                }
                else
                {
                    builder.Append($"User Id={Username};");
                    if (!string.IsNullOrEmpty(EncryptedPassword))
                    {
                        var decryptedPassword = DecryptPassword(EncryptedPassword);
                        builder.Append($"Password={decryptedPassword};");
                    }
                }
                
                builder.Append($"Connection Timeout={ConnectionTimeout};");
                builder.Append("TrustServerCertificate=true;");
                break;
        }

        return builder.ToString();
    }

    private string DecryptPassword(string encryptedPassword)
    {
        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedPassword);
            var decryptedBytes = System.Security.Cryptography.ProtectedData.Unprotect(
                encryptedBytes, 
                null, 
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string EncryptPassword(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword))
            return string.Empty;

        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainPassword);
        var encryptedBytes = System.Security.Cryptography.ProtectedData.Protect(
            plainBytes, 
            null, 
            System.Security.Cryptography.DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encryptedBytes);
    }
}

public enum DatabaseType
{
    SqlServer
}

public enum AuthenticationType
{
    Windows,
    SqlServer
}

