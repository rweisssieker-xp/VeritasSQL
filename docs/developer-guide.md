# VeritasSQL Developer Guide

**Version 1.0.0 | Last Updated: 2025-12-02**

---

## Table of Contents

1. [Development Setup](#development-setup)
2. [Project Structure](#project-structure)
3. [Architecture Overview](#architecture-overview)
4. [Core Services](#core-services)
5. [Adding New Features](#adding-new-features)
6. [Testing](#testing)
7. [Debugging](#debugging)
8. [Code Conventions](#code-conventions)
9. [Common Tasks](#common-tasks)

---

## Development Setup

### Prerequisites

- Windows 10/11
- Visual Studio 2022 or VS Code with C# extension
- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or full)
- Git
- OpenAI API Key (for testing AI features)

### Clone and Build

```bash
# Clone repository
git clone https://github.com/your-repo/VeritasSQL.git
cd VeritasSQL

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run --project VeritasSQL.WPF

# Run tests
dotnet test
```

### IDE Setup

#### Visual Studio 2022

1. Open `VeritasSQL.sln`
2. Set `VeritasSQL.WPF` as startup project
3. Press F5 to debug

#### VS Code

1. Open folder in VS Code
2. Install C# extension
3. Use `dotnet run --project VeritasSQL.WPF`

### Configuration for Development

Create a test database connection after first launch, or use the included demo setup:

```bash
# See SETUP_DEMO.md for demo database setup
```

---

## Project Structure

```
VeritasSQL/
├── VeritasSQL.sln              # Solution file
│
├── VeritasSQL.Core/            # Core business logic (class library)
│   ├── Data/                   # Data access layer
│   │   ├── AuditLogger.cs      # Audit trail logging
│   │   ├── ConnectionManager.cs # Connection profile management
│   │   └── HistoryService.cs   # Query history & favorites
│   │
│   ├── Export/                 # Export functionality
│   │   ├── CsvExporter.cs      # CSV export with metadata
│   │   └── ExcelExporter.cs    # Excel export with formatting
│   │
│   ├── Models/                 # Data models
│   │   ├── AppSettings.cs      # Application settings
│   │   ├── ConnectionProfile.cs # Database connection info
│   │   ├── SchemaInfo.cs       # Database schema models
│   │   ├── QueryHistoryEntry.cs # History entry model
│   │   ├── ValidationResult.cs # Validation results
│   │   └── [AI Models]         # 30+ AI feature models
│   │
│   ├── Services/               # Business services
│   │   ├── OpenAIService.cs    # OpenAI API integration
│   │   ├── SchemaService.cs    # Schema discovery
│   │   ├── QueryExecutor.cs    # SQL execution
│   │   ├── SettingsService.cs  # Settings management
│   │   └── DomainDictionaryService.cs # Term mapping
│   │
│   └── Validation/             # Query validation
│       ├── QueryValidator.cs   # Main validator
│       ├── WhitelistValidator.cs
│       ├── BlacklistValidator.cs
│       └── SchemaValidator.cs
│
├── VeritasSQL.WPF/             # WPF desktop application
│   ├── App.xaml(.cs)           # Application entry point & DI
│   ├── MainWindow.xaml(.cs)    # Main window UI
│   │
│   ├── ViewModels/             # MVVM ViewModels
│   │   └── MainViewModel.cs    # Main application logic
│   │
│   ├── Dialogs/                # Dialog windows
│   │   ├── ConnectionDialog.xaml
│   │   ├── SettingsDialog.xaml
│   │   └── FirstRunWizard.xaml
│   │
│   └── Converters/             # WPF value converters
│       ├── BoolToColorConverter.cs
│       ├── NullToVisibilityConverter.cs
│       └── SeverityToColorConverter.cs
│
├── VeritasSQL.Tests/           # Unit tests
│   └── [Test files]
│
└── docs/                       # Documentation
    ├── index.md
    ├── user-manual.md
    ├── developer-guide.md
    └── architecture.md
```

---

## Architecture Overview

### Layer Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│                     (VeritasSQL.WPF)                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐ │
│  │   Views     │  │  ViewModels │  │   Converters    │ │
│  │   (XAML)    │  │   (MVVM)    │  │                 │ │
│  └─────────────┘  └─────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                    Business Layer                        │
│                    (VeritasSQL.Core)                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐ │
│  │  Services   │  │ Validation  │  │     Export      │ │
│  │             │  │             │  │                 │ │
│  └─────────────┘  └─────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                      Data Layer                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐ │
│  │  SQL Server │  │   SQLite    │  │    OpenAI API   │ │
│  │  (Target DB)│  │  (Local)    │  │                 │ │
│  └─────────────┘  └─────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### Design Patterns

- **MVVM**: Model-View-ViewModel with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Repository Pattern**: Data access abstraction
- **Command Pattern**: RelayCommand for UI actions

### Key Dependencies

| Package | Purpose |
|---------|---------|
| CommunityToolkit.Mvvm | MVVM framework with source generators |
| Microsoft.Data.SqlClient | SQL Server connectivity |
| OpenAI | OpenAI API client |
| AvalonEdit | SQL syntax highlighting editor |
| System.Data.SQLite.Core | Local storage |
| EPPlus | Excel export |
| CsvHelper | CSV export |
| LiveChartsCore | Data visualization |

---

## Core Services

### OpenAIService

Handles all AI-powered features via OpenAI API.

```csharp
public class OpenAIService
{
    // Configure API key and model
    public void Configure(string apiKey, string model = "gpt-4");
    
    // Generate SQL from natural language
    public Task<OpenAIResponse> GenerateSqlAsync(
        string naturalLanguageQuery, 
        SchemaInfo schema,
        Dictionary<string, string>? domainDictionary = null);
    
    // Get query suggestions based on schema
    public Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(SchemaInfo schema);
    
    // Analyze query results for insights
    public Task<DataInsights> AnalyzeDataAsync(DataTable results, string query);
    
    // Optimize SQL query
    public Task<QueryOptimization> OptimizeQueryAsync(string sql, SchemaInfo schema);
    
    // 20+ more AI methods...
}
```

### SchemaService

Discovers database schema information.

```csharp
public class SchemaService
{
    // Load complete schema (tables, views, columns, keys)
    public Task<SchemaInfo> LoadSchemaAsync(string connectionString);
}
```

### QueryExecutor

Executes validated SQL queries.

```csharp
public class QueryExecutor
{
    // Execute query with timeout and row limit
    public Task<QueryResult> ExecuteAsync(
        string sql, 
        string connectionString,
        int timeout = 30,
        int maxRows = 10000);
}
```

### QueryValidator

Multi-layer security validation.

```csharp
public class QueryValidator
{
    // Validate SQL against all security rules
    public ValidationResult Validate(string sql, SchemaInfo schema);
}
```

Validation layers:
1. **Whitelist**: Only SELECT allowed
2. **Blacklist**: Dangerous keywords blocked
3. **Single Statement**: No multiple statements
4. **Schema Gate**: Only known objects
5. **TOP Injection**: Automatic row limits

---

## Adding New Features

### Adding a New AI Feature

1. **Define the model** in `VeritasSQL.Core/Models/`:

```csharp
public class MyNewFeatureResult
{
    public string Analysis { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}
```

2. **Add the service method** in `OpenAIService.cs`:

```csharp
public async Task<MyNewFeatureResult> MyNewFeatureAsync(DataTable data, SchemaInfo schema)
{
    if (string.IsNullOrEmpty(_apiKey))
        throw new InvalidOperationException("OpenAI API Key is not configured");

    var client = new ChatClient(_model, _apiKey);
    
    var systemPrompt = "You are an expert data analyst...";
    var userPrompt = $"Analyze this data: {SerializeData(data)}";
    
    var response = await client.CompleteChatAsync(new[] {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(userPrompt)
    });
    
    return ParseResponse<MyNewFeatureResult>(response);
}
```

3. **Add ViewModel property and command** in `MainViewModel.cs`:

```csharp
[ObservableProperty]
private MyNewFeatureResult? _myNewFeatureResult;

[RelayCommand(CanExecute = nameof(CanExecuteAIFeature))]
private async Task MyNewFeatureAsync()
{
    try
    {
        IsBusy = true;
        MyNewFeatureResult = await _openAIService.MyNewFeatureAsync(QueryResults!, CurrentSchema!);
    }
    finally
    {
        IsBusy = false;
    }
}
```

4. **Add UI** in `MainWindow.xaml`:

```xml
<Button Content="🆕 My Feature"
        Command="{Binding MyNewFeatureCommand}"
        Background="#9C27B0" Foreground="White"/>

<TextBlock Text="{Binding MyNewFeatureResult.Analysis}"
           Visibility="{Binding MyNewFeatureResult, Converter={StaticResource NullToVisibilityConverter}}"/>
```

### Adding a New Export Format

1. Create exporter in `VeritasSQL.Core/Export/`:

```csharp
public class JsonExporter
{
    public async Task ExportAsync(DataTable data, string filePath, ExportMetadata metadata)
    {
        var json = JsonConvert.SerializeObject(new {
            metadata,
            data = DataTableToList(data)
        }, Formatting.Indented);
        
        await File.WriteAllTextAsync(filePath, json);
    }
}
```

2. Register in DI (`App.xaml.cs`):

```csharp
services.AddTransient<JsonExporter>();
```

3. Add command in ViewModel and button in UI.

---

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~ValidationTests"
```

### Test Structure

```
VeritasSQL.Tests/
├── ValidationTests.cs      # Query validation tests
├── SchemaServiceTests.cs   # Schema discovery tests
├── OpenAIServiceTests.cs   # AI service tests (mocked)
└── ExportTests.cs          # Export functionality tests
```

### Writing Tests

```csharp
[Fact]
public void Validate_SelectQuery_ReturnsValid()
{
    // Arrange
    var validator = new QueryValidator();
    var schema = CreateTestSchema();
    var sql = "SELECT * FROM Customers";
    
    // Act
    var result = validator.Validate(sql, schema);
    
    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
}
```

---

## Debugging

### Common Debug Scenarios

#### OpenAI API Issues

```csharp
// Add logging in OpenAIService
System.Diagnostics.Debug.WriteLine($"API Request: {userPrompt}");
System.Diagnostics.Debug.WriteLine($"API Response: {response.Content}");
```

#### SQL Validation Issues

```csharp
// Check validation result details
var result = validator.Validate(sql, schema);
foreach (var error in result.Errors)
{
    Debug.WriteLine($"Validation Error: {error.Message} (Severity: {error.Severity})");
}
```

#### WPF Binding Issues

Enable binding errors in Output window:
```xml
<!-- In App.xaml -->
<Application xmlns:diag="clr-namespace:System.Diagnostics;assembly=WindowsBase">
    <!-- Binding errors will appear in Output window -->
</Application>
```

### Logging

The application uses `System.Diagnostics.Debug` for development logging. Check the Output window in Visual Studio.

---

## Code Conventions

### Naming

- **Classes**: PascalCase (`QueryValidator`)
- **Methods**: PascalCase (`ValidateQuery`)
- **Properties**: PascalCase (`IsConnected`)
- **Private fields**: _camelCase (`_connectionManager`)
- **Parameters**: camelCase (`connectionString`)
- **Constants**: PascalCase (`MaxRowLimit`)

### File Organization

- One class per file (exceptions: small related classes)
- File name matches class name
- Group by feature, not by type

### MVVM Conventions

- ViewModels use `[ObservableProperty]` for bindable properties
- Commands use `[RelayCommand]` attribute
- CanExecute methods named `CanXxx` for command `Xxx`

### Async/Await

- All I/O operations are async
- Use `ConfigureAwait(false)` in library code
- Suffix async methods with `Async`

---

## Common Tasks

### Adding a New Connection Property

1. Add property to `ConnectionProfile.cs`
2. Update `ConnectionDialog.xaml` UI
3. Update connection string builder in `ConnectionManager`

### Adding a New Setting

1. Add property to `AppSettings.cs`
2. Update `SettingsDialog.xaml` UI
3. Handle in `SettingsService`

### Adding a New Validation Rule

1. Create validator class implementing `IQueryValidator`
2. Add to validation pipeline in `QueryValidator`
3. Add tests

### Updating OpenAI Model

Change default in `OpenAIService.cs`:

```csharp
public OpenAIService(string? apiKey = null, string model = "gpt-4")
```

---

## Troubleshooting Development Issues

### Build Errors

**"Package restore failed"**
```bash
dotnet nuget locals all --clear
dotnet restore
```

**"WPF designer not loading"**
- Clean solution
- Delete `bin/` and `obj/` folders
- Rebuild

### Runtime Errors

**"DPAPI encryption failed"**
- Run as administrator once
- Check Windows user profile

**"SQLite database locked"**
- Close other instances
- Check for hanging processes

---

## Resources

- [.NET 8 Documentation](https://docs.microsoft.com/dotnet)
- [WPF Documentation](https://docs.microsoft.com/dotnet/desktop/wpf)
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/windows/communitytoolkit/mvvm)
- [OpenAI API Reference](https://platform.openai.com/docs)
- [AvalonEdit](http://avalonedit.net/)
