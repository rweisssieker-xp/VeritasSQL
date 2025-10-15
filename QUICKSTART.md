# VeritasSQL - Quick Start Guide

## 🚀 Ready in 5 Minutes

### Step 1: Start Project

```bash
cd C:\tmp\VeritasSQL
dotnet run --project VeritasSQL.WPF
```

The application starts and shows the main window.

### Step 2: Setup First Connection

Since no connections exist yet, you need to create a connection profile:

**Option A: Via Code (temporary for demo)**

Create a temporary demo connection via code. Add the following to `MainViewModel.cs` in the `InitializeAsync` method:

```csharp
// Create demo connection (for testing only)
if (profiles.Count == 0)
{
    var demoProfile = new ConnectionProfile
    {
        Name = "Demo SQL Server (LocalDB)",
        Server = "(localdb)\\MSSQLLocalDB",
        Database = "master",
        AuthType = AuthenticationType.Windows,
        ConnectionTimeout = 30
    };
    await _connectionManager.SaveProfileAsync(demoProfile);
    ConnectionProfiles.Add(demoProfile);
}
```

**Option B: Manually (production)**

Currently the menu for creating connections is missing. Add a menu to `MainWindow.xaml` or manually create `connections.json` in `%AppData%\VeritasSQL`:

```json
[
  {
    "Id": "demo-001",
    "Name": "Local Database",
    "DatabaseType": 0,
    "Server": "(localdb)\\MSSQLLocalDB",
    "Database": "AdventureWorks2019",
    "AuthType": 0,
    "ConnectionTimeout": 30,
    "CreatedAt": "2025-10-15T08:00:00Z"
  }
]
```

### Step 3: Configure OpenAI API Key

Create `%AppData%\VeritasSQL\settings.json`:

```json
{
  "EncryptedOpenAIApiKey": null,
  "DefaultRowLimit": 100,
  "MaxRowLimit": 10000,
  "QueryTimeoutSeconds": 30,
  "Language": "en-US",
  "DryRunByDefault": true,
  "ShowExplanations": true,
  "OpenAIModel": "gpt-4"
}
```

**Important**: The API key must be encrypted. Use the application to set it (via Settings dialog).

**Temporary solution for demo**: Set the API key directly in code in `OpenAIService`:

```csharp
public OpenAIService(string? apiKey = "sk-YOUR-API-KEY-HERE", string model = "gpt-4")
```

### Step 4: Connect & Load Schema

1. Select connection from dropdown list
2. Click **"Connect"**
3. Click **"Load Schema"**

The schema should now appear in the left panel.

### Step 5: First Query

Enter in the input field:

```
Show me all tables in the database
```

Or if using AdventureWorks:

```
Show the top 10 products sorted by name
```

Click **"Generate SQL"**.

The generated SQL appears in the SQL editor. Review it and click **"Execute"**.

## 📋 Example Database: AdventureWorks

If you don't have your own database, download AdventureWorks:

### Download

```powershell
# Download AdventureWorks2019
Invoke-WebRequest -Uri "https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks2019.bak" -OutFile "C:\Temp\AdventureWorks2019.bak"
```

### Restore in SQL Server

```sql
USE [master]
RESTORE DATABASE [AdventureWorks2019]
FROM DISK = N'C:\Temp\AdventureWorks2019.bak'
WITH FILE = 1,
MOVE N'AdventureWorks2017' TO N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\AdventureWorks2019.mdf',
MOVE N'AdventureWorks2017_log' TO N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\AdventureWorks2019_log.ldf',
NOUNLOAD, STATS = 5
```

### Example Queries for AdventureWorks

```
Show all products

Show me the top 20 customers by name

Which products cost more than 1000 dollars?

Show all orders from 2014

How many products per category?

Show the 10 most expensive products with their subcategory
```

## 🔧 Troubleshooting

### "No connection profiles available"

→ Create a profile via code or JSON (see Step 2)

### "OpenAI API Key not configured"

→ Set the API key in settings.json (encrypted) or temporarily in code

### "Connection failed"

→ Check:
- SQL Server is running
- Server name is correct
- Database exists
- Windows Authentication has access

### "Schema empty"

→ Check:
- Database user has SELECT rights on INFORMATION_SCHEMA
- Database contains tables/views

## 🎯 Next Steps

1. **Add Menu**: Create menu for managing connections
2. **Settings Dialog**: UI for OpenAI API Key and settings
3. **Test Export**: Export results as CSV/Excel
4. **Use History**: Load previous queries from history
5. **Favorites**: Mark frequently used queries

## 🆘 Support

For questions or issues, open an issue in the repository or see the detailed `README.md`.

## ⚠️ Important Notes

- **API Costs**: OpenAI calls incur costs (~$0.01-0.03 per request with GPT-4)
- **Security**: Use only READ-ONLY database users
- **Encryption**: API keys and passwords are encrypted via DPAPI (current Windows user only)
- **Audit**: All actions are logged in `%AppData%\VeritasSQL\audit.db`
