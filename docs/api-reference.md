# VeritasSQL API Reference

**Version 1.0.0 | Last Updated: 2025-12-02**

---

## Overview

This document provides a complete reference for the VeritasSQL.Core library services and models.

---

## Services

### OpenAIService

The main AI service for natural language to SQL translation and data analysis.

#### Constructor

```csharp
public OpenAIService(string? apiKey = null, string model = "gpt-4")
```

#### Methods

##### Configure

Updates the API key and model configuration.

```csharp
public void Configure(string apiKey, string model = "gpt-4")
```

| Parameter | Type | Description |
|-----------|------|-------------|
| apiKey | string | OpenAI API key |
| model | string | Model name (default: gpt-4) |

---

##### GenerateSqlAsync

Generates SQL from natural language query.

```csharp
public Task<OpenAIResponse> GenerateSqlAsync(
    string naturalLanguageQuery, 
    SchemaInfo schema,
    Dictionary<string, string>? domainDictionary = null)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| naturalLanguageQuery | string | User's question in natural language |
| schema | SchemaInfo | Database schema information |
| domainDictionary | Dictionary | Optional term mappings |

**Returns**: `OpenAIResponse` with generated SQL and explanation.

---

##### GetQuerySuggestionsAsync

Generates AI-powered query suggestions based on schema.

```csharp
public Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(SchemaInfo schema)
```

**Returns**: List of `QuerySuggestion` objects.

---

##### AnalyzeDataAsync

Analyzes query results for insights and patterns.

```csharp
public Task<DataInsights> AnalyzeDataAsync(DataTable results, string originalQuery)
```

**Returns**: `DataInsights` with summary, patterns, and recommendations.

---

##### OptimizeQueryAsync

Suggests performance optimizations for SQL query.

```csharp
public Task<QueryOptimization> OptimizeQueryAsync(string sql, SchemaInfo schema)
```

**Returns**: `QueryOptimization` with rating and suggestions.

---

##### ExplainSchemaAsync

Generates natural language explanation of database schema.

```csharp
public Task<string> ExplainSchemaAsync(SchemaInfo schema, string? focusTable = null)
```

**Returns**: String with schema explanation.

---

##### GetSmartFiltersAsync

Generates filter suggestions for a table.

```csharp
public Task<List<SmartFilter>> GetSmartFiltersAsync(
    string tableName, 
    SchemaInfo schema, 
    DataTable? sampleData = null)
```

**Returns**: List of `SmartFilter` suggestions.

---

##### ExplainSqlErrorAsync

Explains SQL errors and suggests fixes.

```csharp
public Task<SqlErrorExplanation> ExplainSqlErrorAsync(
    string sql, 
    string errorMessage, 
    SchemaInfo schema)
```

**Returns**: `SqlErrorExplanation` with fix suggestions.

---

##### SummarizeResultsAsync

Creates natural language summary of query results.

```csharp
public Task<string> SummarizeResultsAsync(
    DataTable results, 
    string originalQuery)
```

**Returns**: String summary suitable for stakeholders.

---

##### DetectAnomaliesAsync

Detects anomalies in query results.

```csharp
public Task<AnomalyDetectionResult> DetectAnomaliesAsync(
    DataTable results, 
    string query)
```

**Returns**: `AnomalyDetectionResult` with detected issues.

---

##### SemanticSearchHistoryAsync

Searches query history by semantic meaning.

```csharp
public Task<List<QueryHistoryEntry>> SemanticSearchHistoryAsync(
    string searchQuery, 
    List<QueryHistoryEntry> history)
```

**Returns**: Ranked list of matching history entries.

---

##### RecommendVisualizationsAsync

Recommends chart types for data.

```csharp
public Task<VisualizationRecommendation> RecommendVisualizationsAsync(
    DataTable results, 
    string query)
```

**Returns**: `VisualizationRecommendation` with chart suggestions.

---

### SchemaService

Discovers database schema information.

#### LoadSchemaAsync

```csharp
public Task<SchemaInfo> LoadSchemaAsync(string connectionString)
```

Loads complete schema including:
- Tables with columns
- Views with columns
- Primary keys
- Foreign key relationships
- Data types and constraints

**Returns**: `SchemaInfo` object.

---

### QueryExecutor

Executes SQL queries against the database.

#### ExecuteAsync

```csharp
public Task<QueryResult> ExecuteAsync(
    string sql, 
    string connectionString,
    int timeout = 30,
    int maxRows = 10000)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| sql | string | SQL query to execute |
| connectionString | string | Database connection string |
| timeout | int | Query timeout in seconds |
| maxRows | int | Maximum rows to return |

**Returns**: `QueryResult` with DataTable and execution metrics.

---

### QueryValidator

Validates SQL queries for security.

#### Validate

```csharp
public ValidationResult Validate(string sql, SchemaInfo schema)
```

Performs multi-layer validation:
1. Whitelist check (SELECT only)
2. Blacklist check (dangerous keywords)
3. Single statement check
4. Schema gate validation
5. Performance warnings

**Returns**: `ValidationResult` with errors and warnings.

---

### ConnectionManager

Manages database connection profiles.

#### GetProfilesAsync

```csharp
public Task<List<ConnectionProfile>> GetProfilesAsync()
```

**Returns**: List of saved connection profiles.

#### SaveProfileAsync

```csharp
public Task SaveProfileAsync(ConnectionProfile profile)
```

Saves or updates a connection profile with encrypted password.

#### DeleteProfileAsync

```csharp
public Task DeleteProfileAsync(string name)
```

Deletes a connection profile by name.

#### TestConnectionAsync

```csharp
public Task<bool> TestConnectionAsync(ConnectionProfile profile)
```

Tests if connection can be established.

---

### HistoryService

Manages query history and favorites.

#### GetHistoryAsync

```csharp
public Task<List<QueryHistoryEntry>> GetHistoryAsync(int limit = 100)
```

**Returns**: Recent query history entries.

#### AddEntryAsync

```csharp
public Task AddEntryAsync(QueryHistoryEntry entry)
```

Adds a new history entry.

#### GetFavoritesAsync

```csharp
public Task<List<QueryHistoryEntry>> GetFavoritesAsync()
```

**Returns**: All favorited queries.

#### ToggleFavoriteAsync

```csharp
public Task ToggleFavoriteAsync(int id, string? name = null)
```

Toggles favorite status for a history entry.

---

### AuditLogger

Logs all actions for compliance.

#### LogAsync

```csharp
public Task LogAsync(AuditEntry entry)
```

Logs an audit entry with timestamp, user, and action details.

---

## Models

### SchemaInfo

```csharp
public class SchemaInfo
{
    public List<TableInfo> Tables { get; set; }
    public List<ViewInfo> Views { get; set; }
    public List<ForeignKeyInfo> ForeignKeys { get; set; }
}
```

### TableInfo

```csharp
public class TableInfo
{
    public string Schema { get; set; }
    public string Name { get; set; }
    public string FullName => $"{Schema}.{Name}";
    public List<ColumnInfo> Columns { get; set; }
}
```

### ColumnInfo

```csharp
public class ColumnInfo
{
    public string Name { get; set; }
    public string DataType { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public int MaxLength { get; set; }
}
```

### ConnectionProfile

```csharp
public class ConnectionProfile
{
    public string Name { get; set; }
    public string Server { get; set; }
    public string Database { get; set; }
    public AuthenticationType AuthType { get; set; }
    public string? Username { get; set; }
    public string? EncryptedPassword { get; set; }
    
    public string GetConnectionString();
    public void SetPassword(string password);
    public string? GetPassword();
}
```

### OpenAIResponse

```csharp
public class OpenAIResponse
{
    public string Sql { get; set; }
    public string Explanation { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}
```

### QuerySuggestion

```csharp
public class QuerySuggestion
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string NaturalLanguageQuery { get; set; }
    public string Category { get; set; }  // analytics, reporting, data_quality, relationships
    public string Complexity { get; set; } // low, medium, high
}
```

### DataInsights

```csharp
public class DataInsights
{
    public string Summary { get; set; }
    public List<string> Insights { get; set; }
    public List<string> DataQualityIssues { get; set; }
    public List<string> Recommendations { get; set; }
    public DataStatistics Statistics { get; set; }
}
```

### QueryOptimization

```csharp
public class QueryOptimization
{
    public string PerformanceRating { get; set; }  // excellent, good, fair, poor
    public string OptimizedSql { get; set; }
    public List<OptimizationRecommendation> Recommendations { get; set; }
    public string EstimatedImprovement { get; set; }
}
```

### ValidationResult

```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; }
    public List<ValidationWarning> Warnings { get; set; }
}

public class ValidationError
{
    public string Message { get; set; }
    public ValidationSeverity Severity { get; set; }
}

public enum ValidationSeverity
{
    Error,
    Warning,
    Info
}
```

### QueryHistoryEntry

```csharp
public class QueryHistoryEntry
{
    public int Id { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string NaturalLanguageQuery { get; set; }
    public string GeneratedSql { get; set; }
    public string ConnectionProfileName { get; set; }
    public int RowCount { get; set; }
    public double ExecutionTimeMs { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsFavorite { get; set; }
    public string? FavoriteName { get; set; }
}
```

### SmartFilter

```csharp
public class SmartFilter
{
    public string Column { get; set; }
    public string FilterType { get; set; }  // equals, range, in, like, date_range
    public string SuggestedValue { get; set; }
    public string Reason { get; set; }
}
```

### AnomalyDetectionResult

```csharp
public class AnomalyDetectionResult
{
    public List<Anomaly> Anomalies { get; set; }
    public int TotalAnomalies { get; set; }
    public string OverallAssessment { get; set; }
}

public class Anomaly
{
    public string Type { get; set; }
    public string Column { get; set; }
    public string Description { get; set; }
    public string Severity { get; set; }  // high, medium, low
    public string Recommendation { get; set; }
}
```

---

## Enums

### AuthenticationType

```csharp
public enum AuthenticationType
{
    Windows,
    SqlServer
}
```

### ValidationSeverity

```csharp
public enum ValidationSeverity
{
    Error,    // Blocks execution
    Warning,  // Allows execution with caution
    Info      // Informational only
}
```

---

## Error Handling

All async methods may throw:

- `InvalidOperationException`: API key not configured
- `SqlException`: Database connection or query errors
- `HttpRequestException`: OpenAI API communication errors
- `JsonException`: Response parsing errors

Recommended pattern:

```csharp
try
{
    var result = await openAIService.GenerateSqlAsync(query, schema);
    if (!result.Success)
    {
        // Handle AI generation failure
        ShowError(result.Error);
    }
}
catch (InvalidOperationException ex)
{
    // API key not configured
    ShowConfigurationError(ex.Message);
}
catch (Exception ex)
{
    // General error handling
    LogError(ex);
    ShowError("An unexpected error occurred");
}
```
