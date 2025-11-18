# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VeritasSQL is a WPF desktop application that translates natural language questions into secure SQL SELECT queries with 45+ AI-powered features. The application emphasizes enterprise-grade security with a 6-layer validation pipeline and comprehensive audit logging.

**Key Technologies:**
- .NET 8.0 (Windows)
- WPF with MVVM architecture
- OpenAI GPT-4o integration
- SQL Server support (PostgreSQL/MySQL planned)
- xUnit for testing

## Build & Run Commands

### Build the Solution
```powershell
dotnet build
```

### Clean the Solution
```powershell
dotnet clean
```

### Run the WPF Application
```powershell
dotnet run --project VeritasSQL.WPF
```

### Run Tests
```powershell
dotnet test
```

### Run Specific Test
```powershell
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

### Restore Dependencies
```powershell
dotnet restore
```

## Solution Architecture

### Project Structure

The solution consists of three projects:

1. **VeritasSQL.Core** (.NET 8.0) - Business logic library
   - `Models/` - Domain models (35+ classes for all features)
   - `Services/` - Core services (ConnectionManager, SchemaService, OpenAIService, QueryExecutor, SettingsService, DomainDictionaryService)
   - `Validation/` - Query security validation (QueryValidator with 6-layer validation)
   - `Data/` - Data persistence (HistoryService, AuditLogger using SQLite)
   - `Export/` - Export services (CsvExporter, ExcelExporter)

2. **VeritasSQL.WPF** (.NET 8.0-windows) - WPF UI application
   - `ViewModels/` - MVVM ViewModels (MainViewModel with 40+ commands)
   - `Views/` - XAML views
   - `Dialogs/` - Dialog windows
   - `Converters/` - Value converters for data binding

3. **VeritasSQL.Tests** (.NET 8.0) - xUnit test project
   - Uses xUnit test framework
   - Currently minimal test coverage (opportunity for expansion)

### Dependency Injection Setup

The application uses `Microsoft.Extensions.DependencyInjection` for IoC. All services are registered in `App.xaml.cs` (VeritasSQL.WPF/App.xaml.cs:51-79):

- **Singleton Services**: ConnectionManager, SettingsService, SchemaService, QueryExecutor, HistoryService, AuditLogger, DomainDictionaryService, OpenAIService, MainViewModel
- **Transient Services**: CsvExporter, ExcelExporter, MainWindow

**Important**: OpenAIService is initialized with an empty API key on startup to avoid blocking. The key is configured later when the user enters it in settings.

### MVVM Pattern with CommunityToolkit.Mvvm

The application uses the MVVM pattern with source generators from `CommunityToolkit.Mvvm`:

- ViewModels use `[ObservableProperty]` attribute for automatic property change notifications
- Commands use `[RelayCommand]` attribute for ICommand implementations
- MainViewModel contains 40+ commands for all features

## Key Services

### OpenAIService (VeritasSQL.Core/Services/OpenAIService.cs)

Handles all OpenAI API interactions with 27+ async methods:

- `GenerateSqlAsync()` - NL-to-SQL translation
- `GenerateQuerySuggestionsAsync()` - AI query suggestions based on schema
- `AnalyzeDataInsightsAsync()` - Data quality and insights analysis
- `OptimizeQueryAsync()` - Performance optimization recommendations
- `ExplainSchemaRelationshipsAsync()` - Schema relationship explanations
- `GenerateSmartFiltersAsync()` - Data-driven filter suggestions
- `ExplainSqlErrorAsync()` - Error analysis and auto-fix generation
- `GenerateNaturalLanguageSummaryAsync()` - Executive-friendly result summaries
- `DetectDataAnomaliesAsync()` - Anomaly detection in query results
- `SemanticSearchHistoryAsync()` - Semantic search over query history
- `RecommendVisualizationsAsync()` - Chart type recommendations
- `GenerateCopilotSuggestionsAsync()` - Auto-complete suggestions
- `PredictNextQueriesAsync()` - Netflix-style "you might also ask"
- `FindJoinPathAsync()` - Graph-based JOIN path discovery
- `ProfileDataWithPIIDetectionAsync()` - GDPR compliance PII detection
- `ProcessConversationalQueryAsync()` - Multi-turn conversation context
- `GenerateDashboardAsync()` - Auto-generate BI dashboards
- `CalculateDataQualityScoreAsync()` - 0-100 data quality scoring
- `AnalyzeBusinessImpactAsync()` - Schema change impact prediction
- `TranscribeAudioAsync()` - Voice-to-SQL with Whisper
- `EstimateQueryCostAsync()` - Cloud cost prediction
- `FindCorrelationsAsync()` - Statistical correlation analysis
- `RecommendStatisticalTestsAsync()` - Data science test recommendations
- `GenerateDataStoryAsync()` - Data-to-narrative storytelling

All methods use the GPT-4o model by default and require a valid OpenAI API key.

### QueryValidator (VeritasSQL.Core/Validation/QueryValidator.cs)

Implements the 6-layer security validation pipeline:

1. **Whitelist Validation**: Only SELECT statements allowed
2. **Blacklist Validation**: Blocks dangerous keywords (INSERT, UPDATE, DELETE, DROP, ALTER, EXEC, xp_cmdshell, etc.)
3. **Single Statement Check**: Prevents multiple statements (no semicolons)
4. **Schema Gate Validation**: Only approved tables/views/columns can be referenced
5. **TOP Injection**: Automatic addition of row limits (prevents runaway queries)
6. **Performance Warnings**: Missing WHERE clauses, SELECT * usage

Returns `ValidationResult` with `IsValid` flag, list of `ValidationIssue` objects, and `ModifiedSql` with injected limits.

### SchemaService (VeritasSQL.Core/Services/SchemaService.cs)

Loads database schema metadata from SQL Server:
- Tables and views
- Columns (name, data type, nullability, max length)
- Primary keys
- Foreign key relationships

Returns `SchemaInfo` object with all metadata needed for AI features and validation.

### ConnectionManager (VeritasSQL.Core/Services/ConnectionManager.cs)

Manages database connection profiles:
- Stores connection strings with encrypted passwords (Windows DPAPI)
- Supports Windows Authentication and SQL Server Authentication
- Connection testing and validation
- Profiles saved to `%AppData%\VeritasSQL\connections.json`

### HistoryService & AuditLogger (VeritasSQL.Core/Data/)

**HistoryService**: Stores query history in SQLite (`history.db`)
- Tracks natural language query, generated SQL, connection, timestamp, row count, execution time
- Supports favorites with custom names and descriptions
- Searchable and filterable

**AuditLogger**: Comprehensive audit trail in SQLite (`audit.db`)
- Every action logged (Connect, GenerateSQL, ExecuteQuery)
- Forensic details: user, timestamp, action type, data source, NL query, SQL, validation status, execution status, errors
- Tamper-proof append-only design for compliance (SOX, GDPR, HIPAA)

## Application Configuration

All configuration files are stored in `%AppData%\VeritasSQL`:

- `connections.json` - Connection profiles (passwords encrypted via DPAPI)
- `settings.json` - Application settings (OpenAI API key encrypted via DPAPI)
- `domain_dictionary.json` - Business term synonyms (e.g., "customers" → "Customers" table)
- `history.db` - SQLite database for query history and favorites
- `audit.db` - SQLite database for audit logs

**Security Note**: All sensitive data (API keys, passwords) is encrypted using Windows DPAPI with CurrentUser scope. Only the logged-in user can decrypt these secrets.

## Development Guidelines

### When Working with AI Features

1. **OpenAI Integration**: All AI features must go through `OpenAIService`. Never call OpenAI API directly from ViewModels or UI code.

2. **Prompt Engineering**: System prompts are built in `OpenAIService` methods. When modifying prompts:
   - Include schema information for context
   - Use domain dictionary for business term mapping
   - Request structured JSON responses where possible
   - Always ask for explanations/reasoning to improve quality

3. **Error Handling**: AI features should gracefully handle API failures:
   - Check if API key is configured
   - Handle rate limits and network errors
   - Provide user-friendly error messages
   - Never expose raw API errors to users

### When Working with Security/Validation

1. **Always Validate**: Every SQL query MUST pass through `QueryValidator` before execution.

2. **Never Trust AI Output**: Even though GPT-4 generates queries, always validate them. AI can make mistakes.

3. **Read-Only Philosophy**: This application is designed for read-only access. Never add write capabilities without explicit security review.

4. **Audit Everything**: Use `AuditLogger` for all significant actions (connections, query generation, execution).

### When Working with the UI

1. **Async Operations**: All long-running operations (OpenAI calls, database queries) must be async to avoid UI freezing.

2. **Progress Indicators**: Show status bar progress for long operations.

3. **Error Display**: Use ValidationIssue severity levels for color-coded feedback:
   - Error (red) - Blocks execution
   - Warning (orange) - Allowed but risky
   - Info (blue) - Informational

4. **Responsive Design**: The 3-panel layout (Schema | Query/Results | Validation) must remain responsive.

### When Adding New AI Features

The application is designed to support 45+ AI features. When adding new features:

1. Add the model to `VeritasSQL.Core/Models/` (follow existing naming: `FeatureNameResult.cs`)
2. Add the async method to `OpenAIService` (follow naming: `FeatureNameAsync()`)
3. Add the command to `MainViewModel` (use `[RelayCommand]` attribute)
4. Add UI elements to the appropriate tab in MainWindow
5. Update README.md with feature description

**Feature Categories**:
- Proactive AI (Auto-Scheduler, Health Monitor, Drift Detector)
- Analytics AI (What-If, Forecasting, Root Cause)
- Collaborative AI (Team Hub, Query Review, Auto-Documentation)
- Language AI (Multi-Language, Ambiguity Resolver, Synonyms)
- Visual AI (Chart-to-SQL, Data Canvas, Screenshot Analyzer)
- Governance AI (Compliance, Lineage, Access Control)
- Learning AI (SQL Tutor, Best Practices, ELI5)

## Common Development Workflows

### Running the Application for Development

The application requires SQL Server access and an OpenAI API key. When testing:

1. Kill any existing instances first (PowerShell):
   ```powershell
   Get-Process -Name VeritasSQL.WPF -ErrorAction SilentlyContinue | Stop-Process -Force
   ```

2. Clean and rebuild:
   ```powershell
   dotnet clean
   dotnet build
   ```

3. Run the application:
   ```powershell
   dotnet run --project VeritasSQL.WPF
   ```

4. Check for crashes in Event Log:
   ```powershell
   Get-EventLog -LogName Application -Source 'Application Error' -Newest 5 2>&1 | Format-List -Property TimeGenerated,Message
   ```

### Debugging SQL Validation Issues

If queries are incorrectly rejected:

1. Check `QueryValidator` logic in VeritasSQL.Core/Validation/QueryValidator.cs
2. Review the blacklist keywords and whitelist patterns
3. Test with `ValidationResult` object to see specific issues
4. Check schema validation if "unknown table/column" errors occur

### Working with the Schema

Schema is loaded once per connection. To refresh:

1. Disconnect and reconnect in the UI, or
2. Call `SchemaService.LoadSchemaAsync()` programmatically

Schema includes:
- All user tables and views
- Column metadata (data types, nullability, max length)
- Primary key information
- Foreign key relationships (critical for JOIN path finding)

## Known Limitations

- Currently only supports SQL Server (PostgreSQL/MySQL planned)
- No parameter dialog for dynamic query values (planned)
- Test coverage is minimal (opportunity for improvement)
- Voice-to-SQL requires audio file upload (live microphone planned)
- Some advanced AI features (24-45) are documented but not fully implemented yet

## Performance Considerations

- OpenAI API calls can take 2-10 seconds depending on complexity
- Schema loading on large databases can be slow (optimize with table filtering if needed)
- Query execution has default 30-second timeout (configurable in settings)
- SQLite databases (history.db, audit.db) have no size limits - implement cleanup for long-running systems

## Troubleshooting

### Application Won't Start

1. Check that .NET 8.0 is installed
2. Verify Windows 10/11 OS
3. Check Event Log for startup errors
4. Delete `%AppData%\VeritasSQL` to reset configuration (will lose connections and history)

### OpenAI API Errors

1. Verify API key is configured in Settings
2. Check OpenAI account has credits
3. Verify internet connectivity
4. Check if using correct model (gpt-4o)

### Query Validation Failures

1. Check if using SELECT statement (no INSERT/UPDATE/DELETE)
2. Verify table/column names match schema exactly
3. Check for blacklisted keywords in comments or string literals
4. Review ValidationResult.Issues for specific problems

### Connection Issues

1. Test connection in UI before running queries
2. Verify SQL Server is accessible
3. Check Windows/SQL Server authentication credentials
4. Ensure database user has SELECT permissions on target tables
