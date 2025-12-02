# VeritasSQL Architecture

**Version 1.0.0 | Last Updated: 2025-12-02**

---

## Overview

VeritasSQL is a WPF desktop application built on .NET 8 that translates natural language queries into SQL using OpenAI's GPT models. The architecture emphasizes security, extensibility, and separation of concerns.

---

## System Architecture

```mermaid
flowchart TB
    subgraph Presentation["Presentation Layer (WPF)"]
        UI[MainWindow.xaml]
        VM[MainViewModel]
        Dialogs[Dialogs]
        Converters[Value Converters]
    end
    
    subgraph Business["Business Layer (Core)"]
        OpenAI[OpenAIService]
        Schema[SchemaService]
        Executor[QueryExecutor]
        Validator[QueryValidator]
        Export[Exporters]
    end
    
    subgraph Data["Data Layer"]
        SQLServer[(SQL Server)]
        SQLite[(SQLite Local)]
        OpenAIAPI[OpenAI API]
    end
    
    UI --> VM
    VM --> OpenAI
    VM --> Schema
    VM --> Executor
    VM --> Validator
    VM --> Export
    
    OpenAI --> OpenAIAPI
    Schema --> SQLServer
    Executor --> SQLServer
    Validator --> Schema
    Export --> SQLite
```

---

## Component Architecture

### Presentation Layer (VeritasSQL.WPF)

The UI layer follows the MVVM pattern using CommunityToolkit.Mvvm.

```mermaid
classDiagram
    class MainWindow {
        +MainViewModel ViewModel
        +TreeView SchemaTreeView
        +TextEditor SqlEditor
        +DataGrid ResultsGrid
    }
    
    class MainViewModel {
        +ObservableCollection~ConnectionProfile~ ConnectionProfiles
        +SchemaInfo CurrentSchema
        +string NaturalLanguageQuery
        +string GeneratedSql
        +DataTable QueryResults
        +GenerateSqlCommand()
        +ExecuteQueryCommand()
    }
    
    class ConnectionDialog {
        +ConnectionProfile Profile
        +TestConnection()
        +Save()
    }
    
    class SettingsDialog {
        +AppSettings Settings
        +Save()
    }
    
    MainWindow --> MainViewModel
    MainWindow --> ConnectionDialog
    MainWindow --> SettingsDialog
```

### Business Layer (VeritasSQL.Core)

Core business logic organized by responsibility.

```mermaid
classDiagram
    class OpenAIService {
        -string _apiKey
        -string _model
        +Configure(apiKey, model)
        +GenerateSqlAsync(query, schema)
        +GetQuerySuggestionsAsync(schema)
        +AnalyzeDataAsync(results)
        +OptimizeQueryAsync(sql, schema)
    }
    
    class SchemaService {
        +LoadSchemaAsync(connectionString)
    }
    
    class QueryExecutor {
        +ExecuteAsync(sql, connectionString)
    }
    
    class QueryValidator {
        -List~IValidator~ _validators
        +Validate(sql, schema)
    }
    
    class ConnectionManager {
        +GetProfilesAsync()
        +SaveProfileAsync(profile)
        +DeleteProfileAsync(name)
    }
    
    class HistoryService {
        +GetHistoryAsync(limit)
        +AddEntryAsync(entry)
        +GetFavoritesAsync()
    }
    
    OpenAIService --> SchemaInfo
    QueryValidator --> SchemaInfo
    QueryExecutor --> ValidationResult
```

---

## Data Flow

### Query Execution Flow

```mermaid
sequenceDiagram
    participant User
    participant UI as MainWindow
    participant VM as MainViewModel
    participant AI as OpenAIService
    participant Val as QueryValidator
    participant Exec as QueryExecutor
    participant DB as SQL Server
    
    User->>UI: Enter natural language query
    UI->>VM: GenerateSqlCommand
    VM->>AI: GenerateSqlAsync(query, schema)
    AI-->>VM: SQL + Explanation
    VM->>Val: Validate(sql, schema)
    Val-->>VM: ValidationResult
    
    alt Validation Passed
        User->>UI: Click Execute
        UI->>VM: ExecuteQueryCommand
        VM->>Exec: ExecuteAsync(sql)
        Exec->>DB: SELECT query
        DB-->>Exec: Results
        Exec-->>VM: DataTable
        VM-->>UI: Display results
    else Validation Failed
        VM-->>UI: Show errors
    end
```

### Security Validation Pipeline

```mermaid
flowchart LR
    SQL[SQL Query] --> W[Whitelist Check]
    W --> B[Blacklist Check]
    B --> S[Single Statement]
    S --> G[Schema Gate]
    G --> T[TOP Injection]
    T --> R{Result}
    R -->|Pass| Execute[Execute Query]
    R -->|Fail| Block[Block & Report]
```

---

## Security Architecture

### Multi-Layer Validation

| Layer | Purpose | Implementation |
|-------|---------|----------------|
| 1. Whitelist | Only SELECT allowed | Regex pattern matching |
| 2. Blacklist | Block dangerous keywords | Keyword list check |
| 3. Single Statement | Prevent injection | Semicolon detection |
| 4. Schema Gate | Only known objects | Schema metadata validation |
| 5. TOP Injection | Limit result size | Automatic TOP clause |
| 6. Performance | Warn on risky patterns | Heuristic analysis |

### Data Protection

```mermaid
flowchart TB
    subgraph Encryption["Windows DPAPI Encryption"]
        APIKey[OpenAI API Key]
        DBPass[Database Passwords]
    end
    
    subgraph Storage["Secure Storage"]
        Settings[settings.json]
        Connections[connections.json]
    end
    
    subgraph Scope["CurrentUser Scope"]
        User[Windows User Profile]
    end
    
    APIKey --> Settings
    DBPass --> Connections
    Settings --> User
    Connections --> User
```

### Audit Trail

Every action is logged to SQLite with:

- Timestamp (UTC)
- User identity
- Action type (Connect, GenerateSQL, Execute, Export)
- Data source
- Natural language query
- Generated SQL
- Validation status
- Execution result
- Error details (if any)

---

## AI Integration Architecture

### OpenAI Service Methods

```mermaid
mindmap
  root((OpenAIService))
    Core
      GenerateSqlAsync
      ExplainSqlAsync
      RefineSqlAsync
    Suggestions
      GetQuerySuggestionsAsync
      GetSmartFiltersAsync
      PredictNextQueriesAsync
    Analysis
      AnalyzeDataAsync
      DetectAnomaliesAsync
      FindCorrelationsAsync
    Optimization
      OptimizeQueryAsync
      EstimateQueryCostAsync
    Advanced
      ChatAsync
      GenerateDashboardAsync
      ProfileDataAsync
      TranscribeAudioAsync
```

### AI Feature Categories

| Category | Features | Purpose |
|----------|----------|---------|
| **Core NL-to-SQL** | Generate, Explain, Refine | Basic translation |
| **Suggestions** | Query, Filter, Predictive | Proactive assistance |
| **Analysis** | Insights, Anomalies, Correlations | Data understanding |
| **Optimization** | Performance, Cost | Query improvement |
| **Advanced** | Chat, Dashboard, Profiling | Enterprise features |

---

## Dependency Injection

### Service Registration (App.xaml.cs)

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Core Services (Singleton - shared state)
    services.AddSingleton<ConnectionManager>();
    services.AddSingleton<SettingsService>();
    services.AddSingleton<SchemaService>();
    services.AddSingleton<QueryExecutor>();
    services.AddSingleton<HistoryService>();
    services.AddSingleton<AuditLogger>();
    services.AddSingleton<DomainDictionaryService>();
    services.AddSingleton<OpenAIService>();

    // Export Services (Transient - stateless)
    services.AddTransient<CsvExporter>();
    services.AddTransient<ExcelExporter>();

    // ViewModels (Singleton - UI state)
    services.AddSingleton<MainViewModel>();

    // Windows (Transient - new instance each time)
    services.AddTransient<MainWindow>();
}
```

### Service Lifetime Strategy

| Lifetime | Services | Reason |
|----------|----------|--------|
| Singleton | ConnectionManager, SettingsService, OpenAIService | Shared configuration state |
| Singleton | HistoryService, AuditLogger | Single database connection |
| Singleton | MainViewModel | UI state preservation |
| Transient | Exporters | Stateless operations |
| Transient | Dialogs | Fresh instance per use |

---

## Data Models

### Core Models

```mermaid
classDiagram
    class SchemaInfo {
        +List~TableInfo~ Tables
        +List~ViewInfo~ Views
        +List~ForeignKeyInfo~ ForeignKeys
    }
    
    class TableInfo {
        +string Schema
        +string Name
        +string FullName
        +List~ColumnInfo~ Columns
    }
    
    class ColumnInfo {
        +string Name
        +string DataType
        +bool IsNullable
        +bool IsPrimaryKey
        +int MaxLength
    }
    
    class ConnectionProfile {
        +string Name
        +string Server
        +string Database
        +AuthenticationType AuthType
        +string EncryptedPassword
    }
    
    class QueryHistoryEntry {
        +int Id
        +DateTime ExecutedAt
        +string NaturalLanguageQuery
        +string GeneratedSql
        +string ConnectionProfileName
        +int RowCount
        +double ExecutionTimeMs
        +bool IsSuccess
        +bool IsFavorite
    }
    
    SchemaInfo --> TableInfo
    TableInfo --> ColumnInfo
```

### AI Response Models

```mermaid
classDiagram
    class OpenAIResponse {
        +string Sql
        +string Explanation
        +bool Success
        +string Error
    }
    
    class QuerySuggestion {
        +string Title
        +string Description
        +string NaturalLanguageQuery
        +string Category
        +string Complexity
    }
    
    class DataInsights {
        +string Summary
        +List~string~ Insights
        +List~string~ DataQualityIssues
        +List~string~ Recommendations
    }
    
    class QueryOptimization {
        +string PerformanceRating
        +string OptimizedSql
        +List~Recommendation~ Recommendations
        +string EstimatedImprovement
    }
```

---

## Storage Architecture

### Local Storage (SQLite)

```mermaid
erDiagram
    HISTORY {
        int Id PK
        datetime ExecutedAt
        string NaturalLanguageQuery
        string GeneratedSql
        string ConnectionProfileName
        int RowCount
        float ExecutionTimeMs
        bool IsSuccess
        string ErrorMessage
        bool IsFavorite
        string FavoriteName
    }
    
    AUDIT {
        int Id PK
        datetime Timestamp
        string User
        string ActionType
        string DataSource
        string NaturalLanguageQuery
        string GeneratedSql
        string ValidationStatus
        string ExecutionStatus
        string ErrorDetails
    }
```

### File Storage

```
%AppData%\VeritasSQL\
├── connections.json      # Encrypted connection profiles
├── settings.json         # App settings (encrypted API key)
├── domain_dictionary.json # Custom term mappings
├── history.db            # SQLite query history
└── audit.db              # SQLite audit log
```

---

## Performance Considerations

### Async Operations

All I/O operations are asynchronous to keep the UI responsive:

- Database queries
- OpenAI API calls
- File operations
- Schema loading

### Caching Strategy

| Data | Cache Location | Invalidation |
|------|----------------|--------------|
| Schema | Memory (SchemaInfo) | Manual reload |
| Connection profiles | Memory + File | On save |
| Settings | Memory + File | On save |
| History | SQLite | Never (append-only) |

### Query Optimization

- Automatic TOP clause injection
- Configurable timeout (default: 30s)
- Row limit enforcement (default: 100, max: 10,000)

---

## Extensibility Points

### Adding New AI Features

1. Add model class in `Models/`
2. Add method in `OpenAIService`
3. Add property/command in `MainViewModel`
4. Add UI in `MainWindow.xaml`

### Adding New Export Formats

1. Create exporter class in `Export/`
2. Register in DI container
3. Add command in ViewModel

### Adding New Validation Rules

1. Implement `IQueryValidator` interface
2. Add to validation pipeline
3. Define severity level

### Adding Database Support

1. Create provider-specific `SchemaService`
2. Create provider-specific `QueryExecutor`
3. Update connection dialog

---

## Technology Decisions

### Why WPF?

- Rich desktop UI capabilities
- Native Windows integration (DPAPI)
- Mature ecosystem
- MVVM support

### Why .NET 8?

- Latest LTS version
- Performance improvements
- Modern C# features
- Cross-platform potential (future)

### Why SQLite for Local Storage?

- Zero configuration
- Single file database
- ACID compliance
- Excellent performance

### Why OpenAI GPT-4?

- Best-in-class natural language understanding
- Reliable SQL generation
- Context awareness
- Continuous improvements

---

## Future Architecture Considerations

### Planned Enhancements

- **PostgreSQL/MySQL Support**: Abstract database provider
- **Plugin System**: Dynamic feature loading
- **Cloud Sync**: Optional settings synchronization
- **Multi-Language UI**: Resource-based localization

### Scalability Path

```mermaid
flowchart LR
    Current[Desktop App] --> Web[Web Version]
    Web --> API[API Service]
    API --> Multi[Multi-Tenant]
```
