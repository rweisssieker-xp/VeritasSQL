using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using OpenAI.Audio;
using VeritasSQL.Core.Models;

namespace VeritasSQL.Core.Services;

/// <summary>
/// Communicates with OpenAI API for NL-to-SQL translation
/// </summary>
public class OpenAIService
{
    private string? _apiKey;
    private string _model;

    public OpenAIService(string? apiKey = null, string model = "gpt-4o")
    {
        _apiKey = apiKey;
        _model = model;
    }

    /// <summary>
    /// Updates the API key and model (useful for late initialization)
    /// </summary>
    public void Configure(string apiKey, string model = "gpt-4o")
    {
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<OpenAIResponse> GenerateSqlAsync(
        string naturalLanguageQuery, 
        SchemaInfo schema,
        Dictionary<string, string>? domainDictionary = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var systemPrompt = BuildSystemPrompt(schema, domainDictionary);
        var userPrompt = BuildUserPrompt(naturalLanguageQuery);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            // Parse JSON response
            return ParseOpenAIResponse(content);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error with OpenAI request: {ex.Message}", ex);
        }
    }

    public async Task<string> ExplainSqlAsync(string sql)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var systemPrompt = "You are an SQL expert. Explain SQL queries in clear, understandable language.";
        var userPrompt = $"Explain the following SQL statement:\n\n{sql}";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error with OpenAI request: {ex.Message}", ex);
        }
    }

    public async Task<OpenAIResponse> RefineSqlAsync(
        string currentSql,
        string refinementRequest,
        SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var systemPrompt = BuildSystemPrompt(schema, null);
        var userPrompt = $"Current SQL:\n{currentSql}\n\nRefinement request:\n{refinementRequest}\n\nGenerate the improved SQL.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;
            return ParseOpenAIResponse(content);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error with OpenAI request: {ex.Message}", ex);
        }
    }

    private string BuildSystemPrompt(SchemaInfo schema, Dictionary<string, string>? domainDictionary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an SQL expert for Microsoft SQL Server.");
        sb.AppendLine("Generate ONLY valid SELECT statements. No DML (INSERT, UPDATE, DELETE) or DDL (CREATE, DROP, ALTER).");
        sb.AppendLine();
        sb.AppendLine("CRITICAL RULES:");
        sb.AppendLine("1. Use fully qualified table names (Schema.Table)");
        sb.AppendLine("2. Always add TOP 100 to prevent large result sets");
        sb.AppendLine("3. Only use tables/views that exist in the provided schema");
        sb.AppendLine("4. Use proper SQL Server syntax");
        sb.AppendLine("5. No multiple statements (no semicolons)");
        sb.AppendLine("6. No dangerous functions (EXEC, xp_cmdshell, etc.)");
        sb.AppendLine("7. Use proper JOIN syntax");
        sb.AppendLine("8. Use proper WHERE clause syntax");
        sb.AppendLine("9. Use proper ORDER BY syntax");
        sb.AppendLine("10. Ensure all column names exist in the referenced tables");
        sb.AppendLine();
        sb.AppendLine("Database Schema:");
        sb.AppendLine(SerializeSchema(schema));

        if (domainDictionary != null && domainDictionary.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Domain Dictionary (Synonyms):");
            foreach (var entry in domainDictionary)
            {
                sb.AppendLine($"- '{entry.Key}' → {entry.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("IMPORTANT: Respond ONLY in the following JSON format (no additional text):");
        sb.AppendLine("{");
        sb.AppendLine("  \"sql\": \"SELECT TOP 100 Schema.Table.Column1, Schema.Table.Column2 FROM Schema.Table WHERE condition\",");
        sb.AppendLine("  \"explanation\": \"This query finds...\",");
        sb.AppendLine("  \"tables_used\": [\"Schema.Table1\", \"Schema.Table2\"],");
        sb.AppendLine("  \"estimated_cost\": \"low\"");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("EXAMPLE for 'show me all customers':");
        sb.AppendLine("{");
        sb.AppendLine("  \"sql\": \"SELECT TOP 100 dbo.Customer.CustomerID, dbo.Customer.CustomerName, dbo.Customer.Email FROM dbo.Customer\",");
        sb.AppendLine("  \"explanation\": \"This query retrieves all customer information including ID, name, and email address.\",");
        sb.AppendLine("  \"tables_used\": [\"dbo.Customer\"],");
        sb.AppendLine("  \"estimated_cost\": \"low\"");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string BuildUserPrompt(string naturalLanguageQuery)
    {
        return $"Question: {naturalLanguageQuery}";
    }

    private string SerializeSchema(SchemaInfo schema)
    {
        var sb = new StringBuilder();

        foreach (var table in schema.Tables.Take(50)) // Limit to 50 tables for token limit
        {
            sb.AppendLine($"Table: {table.FullName}");
            foreach (var col in table.Columns)
            {
                var pkMarker = col.IsPrimaryKey ? " [PK]" : "";
                var nullMarker = col.IsNullable ? " NULL" : " NOT NULL";
                sb.AppendLine($"  - {col.Name}: {col.DataType}{nullMarker}{pkMarker}");
            }

            if (table.ForeignKeys.Any())
            {
                foreach (var fk in table.ForeignKeys)
                {
                    sb.AppendLine($"  FK: {fk.ColumnName} → {fk.ReferencedTable}.{fk.ReferencedColumn}");
                }
            }
            sb.AppendLine();
        }

        foreach (var view in schema.Views.Take(20)) // Limit to 20 views
        {
            sb.AppendLine($"View: {view.FullName}");
            foreach (var col in view.Columns)
            {
                sb.AppendLine($"  - {col.Name}: {col.DataType}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates intelligent query suggestions based on database schema
    /// </summary>
    public async Task<List<QuerySuggestion>> GenerateQuerySuggestionsAsync(SchemaInfo schema, int maxSuggestions = 5)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var systemPrompt = @"You are an SQL expert. Analyze the database schema and suggest useful, practical queries that would provide business value.
Focus on:
- Common analytical queries (aggregations, trends, top N)
- Queries that leverage foreign key relationships (JOINs)
- Queries that answer typical business questions
- Data quality checks

Respond with a JSON array of suggestions:
[
  {
    ""title"": ""Short descriptive title"",
    ""description"": ""What business question this answers"",
    ""naturalLanguageQuery"": ""The query in plain English"",
    ""complexity"": ""low|medium|high"",
    ""category"": ""analytics|reporting|data_quality|relationships""
  }
]";

        var userPrompt = $"Database Schema:\n{SerializeSchema(schema)}\n\nGenerate {maxSuggestions} useful query suggestions.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            // Extract JSON array
            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\[[\s\S]*\]",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var suggestions = JsonSerializer.Deserialize<List<QuerySuggestion>>(jsonMatch.Value, options);
                return suggestions ?? new List<QuerySuggestion>();
            }

            return new List<QuerySuggestion>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error generating query suggestions: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Analyzes query results and provides AI-powered data insights
    /// </summary>
    public async Task<DataInsights> AnalyzeDataInsightsAsync(System.Data.DataTable data, string sql, int maxRows = 100)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        // Sample data for analysis (limit to avoid token limits)
        var sampleData = SerializeDataTableSample(data, maxRows);

        var systemPrompt = @"You are a data analyst expert. Analyze the query results and provide actionable insights.
Look for:
- Data quality issues (nulls, duplicates, outliers)
- Statistical patterns and anomalies
- Business insights and trends
- Recommendations for data cleanup or further analysis

Respond in JSON format:
{
  ""summary"": ""Brief overview of the data"",
  ""insights"": [""insight 1"", ""insight 2"", ...],
  ""dataQualityIssues"": [""issue 1"", ""issue 2"", ...],
  ""recommendations"": [""recommendation 1"", ...],
  ""statistics"": {
    ""nullPercentage"": 0.0,
    ""uniqueValues"": 0,
    ""potentialDuplicates"": 0
  }
}";

        var userPrompt = $"SQL Query:\n{sql}\n\nData Sample ({data.Rows.Count} rows total, showing first {Math.Min(maxRows, data.Rows.Count)}):\n{sampleData}\n\nProvide insights.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\{[\s\S]*\}",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var insights = JsonSerializer.Deserialize<DataInsights>(jsonMatch.Value, options);
                return insights ?? new DataInsights();
            }

            return new DataInsights { Summary = "Could not parse AI insights" };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error analyzing data insights: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Provides AI-powered query optimization recommendations
    /// </summary>
    public async Task<QueryOptimization> OptimizeQueryAsync(string sql, SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var systemPrompt = @"You are an SQL performance optimization expert for Microsoft SQL Server.
Analyze the query and provide optimization recommendations focusing on:
- Index suggestions
- Query rewriting for better performance
- JOIN optimization
- Avoiding SELECT *
- Proper use of WHERE clauses
- Avoiding N+1 queries

Respond in JSON format:
{
  ""performanceRating"": ""excellent|good|fair|poor"",
  ""optimizedSql"": ""Optimized version of the query"",
  ""recommendations"": [
    {
      ""type"": ""index|rewrite|join|general"",
      ""priority"": ""high|medium|low"",
      ""issue"": ""Description of the issue"",
      ""suggestion"": ""How to fix it"",
      ""impact"": ""Expected performance impact""
    }
  ],
  ""estimatedImprovement"": ""Estimated performance gain""
}";

        var userPrompt = $"Schema:\n{SerializeSchema(schema)}\n\nSQL Query to optimize:\n{sql}";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\{[\s\S]*\}",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var optimization = JsonSerializer.Deserialize<QueryOptimization>(jsonMatch.Value, options);
                return optimization ?? new QueryOptimization { PerformanceRating = "unknown" };
            }

            return new QueryOptimization { PerformanceRating = "unknown" };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error optimizing query: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Explains complex schema relationships using AI
    /// </summary>
    public async Task<string> ExplainSchemaRelationshipsAsync(SchemaInfo schema, string? focusTable = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var systemPrompt = @"You are a database architect expert. Explain the database schema and relationships in clear, business-friendly language.
Focus on:
- How tables are related via foreign keys
- The business meaning of relationships
- Common query patterns
- Data model structure (fact/dimension tables, etc.)";

        var userPrompt = focusTable != null
            ? $"Schema:\n{SerializeSchema(schema)}\n\nExplain the relationships and usage of table '{focusTable}' in detail."
            : $"Schema:\n{SerializeSchema(schema)}\n\nProvide an overview of the database structure and key relationships.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error explaining schema relationships: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates smart filter suggestions based on data distribution
    /// </summary>
    public async Task<List<SmartFilter>> GenerateSmartFiltersAsync(
        string tableName,
        System.Data.DataTable sampleData,
        SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var sampleDataStr = SerializeDataTableSample(sampleData, 20);

        var systemPrompt = @"You are a data analysis expert. Based on the sample data, suggest useful filters for querying.
Consider:
- Common filter values (categories, status codes, date ranges)
- Outlier detection (min/max values worth filtering)
- Useful aggregation groupings

Respond with a JSON array:
[
  {
    ""column"": ""ColumnName"",
    ""filterType"": ""equals|range|in|like|date_range"",
    ""suggestedValue"": ""Suggested filter value or example"",
    ""reason"": ""Why this filter is useful""
  }
]";

        var userPrompt = $"Table: {tableName}\nSchema: {SerializeSchema(schema)}\n\nSample Data:\n{sampleDataStr}\n\nSuggest useful filters.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\[[\s\S]*\]",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var filters = JsonSerializer.Deserialize<List<SmartFilter>>(jsonMatch.Value, options);
                return filters ?? new List<SmartFilter>();
            }

            return new List<SmartFilter>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error generating smart filters: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Explains SQL errors and provides auto-fix suggestions
    /// </summary>
    public async Task<SqlErrorAnalysis> ExplainAndFixSqlErrorAsync(string sql, string errorMessage, SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var systemPrompt = @"You are an SQL error diagnosis expert. Analyze SQL errors and provide:
1. Clear explanation of what went wrong
2. The root cause of the error
3. Corrected SQL that fixes the issue
4. Learning points to avoid similar errors

Respond in JSON format:
{
  ""errorType"": ""syntax|permission|schema|logic|other"",
  ""explanation"": ""User-friendly explanation of the error"",
  ""rootCause"": ""Technical root cause"",
  ""fixedSql"": ""Corrected SQL statement"",
  ""learningPoints"": [""tip 1"", ""tip 2"", ...],
  ""severity"": ""critical|high|medium|low""
}";

        var userPrompt = $"Schema:\n{SerializeSchema(schema)}\n\nFailed SQL:\n{sql}\n\nError Message:\n{errorMessage}\n\nAnalyze and fix this error.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\{[\s\S]*\}",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var analysis = JsonSerializer.Deserialize<SqlErrorAnalysis>(jsonMatch.Value, options);
                return analysis ?? new SqlErrorAnalysis { ErrorType = "unknown", Explanation = "Could not analyze error" };
            }

            return new SqlErrorAnalysis { ErrorType = "unknown", Explanation = content };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error analyzing SQL error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates a natural language summary of query results
    /// </summary>
    public async Task<string> SummarizeResultsAsync(System.Data.DataTable data, string sql, string naturalLanguageQuery)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var sampleData = SerializeDataTableSample(data, 50);

        var systemPrompt = @"You are a data analysis expert. Summarize query results in clear, business-friendly language.
Focus on:
- Key findings and insights
- Notable patterns or trends
- Data distribution highlights
- Answering the user's original question

Write 2-3 paragraphs in natural, conversational language.";

        var userPrompt = $"User's Question: {naturalLanguageQuery}\n\nSQL Query:\n{sql}\n\nResults ({data.Rows.Count} rows total):\n{sampleData}\n\nProvide a natural language summary.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error summarizing results: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Detects anomalies in query results using AI
    /// </summary>
    public async Task<AnomalyDetectionResult> DetectAnomaliesAsync(System.Data.DataTable data, string sql)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var sampleData = SerializeDataTableSample(data, 100);

        var systemPrompt = @"You are a data anomaly detection expert. Analyze query results for unusual patterns:
- Outliers (values significantly different from the norm)
- Missing data patterns
- Unexpected distributions
- Suspicious values
- Data integrity issues

Respond in JSON format:
{
  ""hasAnomalies"": true/false,
  ""anomalyCount"": 0,
  ""anomalies"": [
    {
      ""type"": ""outlier|missing_data|suspicious_value|integrity_issue"",
      ""description"": ""What the anomaly is"",
      ""affectedRows"": ""Row identifiers or count"",
      ""severity"": ""high|medium|low"",
      ""recommendation"": ""What to do about it""
    }
  ],
  ""summary"": ""Overall assessment""
}";

        var userPrompt = $"SQL Query:\n{sql}\n\nData ({data.Rows.Count} rows):\n{sampleData}\n\nDetect any anomalies.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\{[\s\S]*\}",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<AnomalyDetectionResult>(jsonMatch.Value, options);
                return result ?? new AnomalyDetectionResult { HasAnomalies = false };
            }

            return new AnomalyDetectionResult { HasAnomalies = false, Summary = "Could not analyze anomalies" };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error detecting anomalies: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Semantic search through query history using AI
    /// </summary>
    public async Task<List<int>> SemanticSearchHistoryAsync(string searchQuery, List<QueryHistoryEntry> historyEntries)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        // Limit to recent 100 entries for token management
        var recentHistory = historyEntries.Take(100).ToList();
        var historyText = string.Join("\n", recentHistory.Select((e, i) =>
            $"{i}: {e.NaturalLanguageQuery} | SQL: {(e.GeneratedSql != null ? e.GeneratedSql.Substring(0, Math.Min(100, e.GeneratedSql.Length)) : "")}"));

        var systemPrompt = @"You are a semantic search expert. Find queries that match the user's intent, even if worded differently.
Consider:
- Semantic similarity (same meaning, different words)
- Related queries (similar business questions)
- SQL pattern matching

Return a JSON array of matching indices (0-based) ordered by relevance:
[0, 5, 12, ...]

Return only the top 10 most relevant matches.";

        var userPrompt = $"Search Query: {searchQuery}\n\nHistory:\n{historyText}\n\nReturn matching indices.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\[[\s\S]*?\]",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var indices = JsonSerializer.Deserialize<List<int>>(jsonMatch.Value, options);
                return indices ?? new List<int>();
            }

            return new List<int>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error in semantic search: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Suggests appropriate visualizations for query results
    /// </summary>
    public async Task<VisualizationRecommendations> RecommendVisualizationsAsync(System.Data.DataTable data, string sql)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var sampleData = SerializeDataTableSample(data, 20);
        var columnInfo = string.Join(", ", data.Columns.Cast<System.Data.DataColumn>().Select(c => $"{c.ColumnName}:{c.DataType.Name}"));

        var systemPrompt = @"You are a data visualization expert. Recommend appropriate chart types for query results.
Consider:
- Data types (numeric, categorical, temporal)
- Number of rows and columns
- Relationships and patterns
- Best practices for data visualization

Respond in JSON format:
{
  ""primaryRecommendation"": {
    ""chartType"": ""bar|line|pie|scatter|table|heatmap|area"",
    ""reason"": ""Why this chart type"",
    ""xAxis"": ""Column name for X axis"",
    ""yAxis"": ""Column name for Y axis"",
    ""configuration"": ""Specific configuration tips""
  },
  ""alternativeRecommendations"": [
    {
      ""chartType"": ""..."",
      ""reason"": ""..."",
      ""useCase"": ""When to use this instead""
    }
  ],
  ""insights"": ""What the visualization will reveal""
}";

        var userPrompt = $"SQL Query:\n{sql}\n\nColumns: {columnInfo}\n\nSample Data:\n{sampleData}\n\nRecommend visualizations.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\{[\s\S]*\}",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var recommendations = JsonSerializer.Deserialize<VisualizationRecommendations>(jsonMatch.Value, options);
                return recommendations ?? new VisualizationRecommendations();
            }

            return new VisualizationRecommendations();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error recommending visualizations: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// AI Query Co-Pilot: Provides real-time auto-complete suggestions
    /// </summary>
    public async Task<List<QueryCopilotSuggestion>> GetCopilotSuggestionsAsync(
        string partialQuery,
        SchemaInfo schema,
        List<QueryHistoryEntry> recentHistory)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var recentContext = string.Join("\n", recentHistory.Take(5).Select(h => $"- {h.NaturalLanguageQuery}"));

        var prompt = $@"You are an AI SQL co-pilot. The user is typing a query. Provide 3 smart completion suggestions.

User's partial query: ""{partialQuery}""

Recent queries for context:
{recentContext}

Available schema:
{SerializeSchema(schema)}

Return JSON array with 3 suggestions:
{{
  ""suggestions"": [
    {{
      ""completionText"": ""suggested completion"",
      ""fullSql"": ""complete SQL query"",
      ""explanation"": ""what this does"",
      ""confidence"": 0.95,
      ""category"": ""completion|correction|enhancement""
    }}
  ]
}}

Focus on: common patterns, corrections, enhancements.";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<CopilotResponse>(jsonMatch.Value, options);
                return result?.Suggestions ?? new List<QueryCopilotSuggestion>();
            }

            return new List<QueryCopilotSuggestion>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting co-pilot suggestions: {ex.Message}", ex);
        }
    }

    private class CopilotResponse
    {
        public List<QueryCopilotSuggestion> Suggestions { get; set; } = new();
    }

    /// <summary>
    /// Predictive Next Query: Netflix-style query recommendations
    /// </summary>
    public async Task<List<PredictiveQuerySuggestion>> PredictNextQueriesAsync(
        QueryHistoryEntry lastQuery,
        List<QueryHistoryEntry> history,
        SchemaInfo schema,
        System.Data.DataTable? lastResult = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var historyContext = string.Join("\n", history.Take(10).Select(h =>
            $"- {h.NaturalLanguageQuery} ({h.ExecutedAt:HH:mm})"));

        var resultSample = lastResult != null ? SerializeDataTableSample(lastResult, 5) : "No result available";

        var prompt = $@"You are an AI data analyst. Based on the user's last query and results, predict what they might want to query next.

Last query: ""{lastQuery.NaturalLanguageQuery}""
Last SQL: {lastQuery.GeneratedSql}

Last result sample:
{resultSample}

Recent history:
{historyContext}

Available schema:
{SerializeSchema(schema)}

Return JSON with 5 predictive suggestions:
{{
  ""suggestions"": [
    {{
      ""title"": ""Short title"",
      ""description"": ""What this query does"",
      ""naturalLanguageQuery"": ""Natural language version"",
      ""sql"": ""Complete SQL"",
      ""reasoning"": ""Why this is relevant"",
      ""relevanceScore"": 0.95,
      ""category"": ""follow_up|related|drill_down|comparison""
    }}
  ]
}}

Think like Netflix recommendations: follow-ups, drill-downs, comparisons, related analyses.";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<PredictiveResponse>(jsonMatch.Value, options);
                return result?.Suggestions ?? new List<PredictiveQuerySuggestion>();
            }

            return new List<PredictiveQuerySuggestion>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error predicting next queries: {ex.Message}", ex);
        }
    }

    private class PredictiveResponse
    {
        public List<PredictiveQuerySuggestion> Suggestions { get; set; } = new();
    }

    /// <summary>
    /// Smart JOIN Path Finder: Finds optimal path between tables
    /// </summary>
    public async Task<JoinPathResult> FindJoinPathAsync(
        string sourceTable,
        string targetTable,
        SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var prompt = $@"You are a database relationship expert. Find the optimal JOIN path between two tables.

Source table: {sourceTable}
Target table: {targetTable}

Available schema with relationships:
{SerializeSchema(schema)}

Return JSON with the optimal path:
{{
  ""sourceTable"": ""{sourceTable}"",
  ""targetTable"": ""{targetTable}"",
  ""path"": [
    {{
      ""fromTable"": ""table1"",
      ""toTable"": ""table2"",
      ""joinType"": ""INNER"",
      ""onClause"": ""table1.id = table2.table1_id"",
      ""relationship"": ""one_to_many""
    }}
  ],
  ""completeSql"": ""SELECT * FROM {sourceTable} ... JOIN {targetTable}"",
  ""pathLength"": 1,
  ""explanation"": ""Why this is the best path"",
  ""alternativePaths"": [""alternative explanation""]
}}

Use foreign keys and shortest path. Prefer INNER JOIN unless needed.";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<JoinPathResult>(jsonMatch.Value, options);
                return result ?? new JoinPathResult();
            }

            return new JoinPathResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error finding JOIN path: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// AI Data Profiling & PII Detection
    /// </summary>
    public async Task<DataProfilingResult> ProfileDataAsync(
        string tableName,
        System.Data.DataTable sampleData,
        SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var dataSample = SerializeDataTableSample(sampleData, 20);

        var prompt = $@"You are a data privacy and quality expert. Analyze this data for:
1. PII (Personal Identifiable Information) detection
2. GDPR compliance risks
3. Data quality issues

Table: {tableName}

Sample data:
{dataSample}

Schema info:
{SerializeSchema(schema)}

Return comprehensive JSON:
{{
  ""tableName"": ""{tableName}"",
  ""totalRows"": {sampleData.Rows.Count},
  ""totalColumns"": {sampleData.Columns.Count},
  ""columnProfiles"": [
    {{
      ""columnName"": ""name"",
      ""dataType"": ""string"",
      ""distinctValues"": 100,
      ""nullPercentage"": 0.5,
      ""minValue"": ""A"",
      ""maxValue"": ""Z"",
      ""mostCommonValue"": ""John"",
      ""dataPatterns"": [""capitalized names""],
      ""isPotentiallyPii"": true
    }}
  ],
  ""piiFindings"": [
    {{
      ""columnName"": ""email"",
      ""piiType"": ""email"",
      ""confidence"": 0.99,
      ""recommendation"": ""Encrypt or mask"",
      ""gdprCategory"": ""personal_data"",
      ""sampleValues"": [""j***@example.com""]
    }}
  ],
  ""qualitySummary"": {{
    ""completenessScore"": 95.5,
    ""uniquenessScore"": 88.0,
    ""consistencyScore"": 92.0,
    ""validityScore"": 98.0,
    ""qualityIssues"": [""5% null values in required field""]
  }},
  ""complianceWarnings"": [""Contains email addresses - GDPR applies""],
  ""overallRisk"": ""medium""
}}

Check for: emails, phones, SSNs, credit cards, addresses, names, IP addresses.";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<DataProfilingResult>(jsonMatch.Value, options);
                return result ?? new DataProfilingResult();
            }

            return new DataProfilingResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error profiling data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Conversational Chat: Multi-turn context-aware interactions
    /// </summary>
    public async Task<ChatResponse> ChatAsync(
        string userMessage,
        ConversationContext context,
        SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var conversationHistory = string.Join("\n", context.Turns.Select(t =>
            $"User: {t.UserMessage}\nAssistant: {t.AssistantResponse}\n"));

        var variables = string.Join(", ", context.Variables.Select(kv => $"{kv.Key}={kv.Value}"));

        var systemPrompt = $@"You are a conversational SQL assistant. Maintain context across the conversation.

Available schema:
{SerializeSchema(schema)}

Conversation history:
{conversationHistory}

User-defined variables: {variables}
Referenced tables: {string.Join(", ", context.ReferencedTables)}

Guidelines:
- Remember previous queries and results
- Use pronouns (""it"", ""that table"", ""those results"")
- Ask clarifying questions if needed
- Suggest related follow-ups
- Track user's intent";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            // Try to extract structured response
            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            ChatResponse chatResponse;

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                chatResponse = JsonSerializer.Deserialize<ChatResponse>(jsonMatch.Value, options) ?? new ChatResponse();
            }
            else
            {
                // Plain text response
                chatResponse = new ChatResponse { Message = content };
            }

            // Update context
            context.Turns.Add(new ConversationTurn
            {
                UserMessage = userMessage,
                AssistantResponse = chatResponse.Message,
                GeneratedSql = chatResponse.Sql
            });

            return chatResponse;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error in chat: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Automated Dashboard Generator
    /// </summary>
    public async Task<DashboardDefinition> GenerateDashboardAsync(
        string topic,
        SchemaInfo schema,
        List<QueryHistoryEntry>? recentQueries = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var queryContext = recentQueries != null
            ? string.Join("\n", recentQueries.Take(5).Select(q => $"- {q.NaturalLanguageQuery}"))
            : "No recent queries";

        var prompt = $@"You are a BI dashboard expert. Create an automated dashboard for: ""{topic}""

Available schema:
{SerializeSchema(schema)}

Recent user queries for context:
{queryContext}

Generate a comprehensive dashboard with 6-8 widgets (KPIs, charts, tables).

Return JSON:
{{
  ""title"": ""Dashboard Title"",
  ""description"": ""What this dashboard shows"",
  ""widgets"": [
    {{
      ""title"": ""Total Revenue"",
      ""widgetType"": ""kpi"",
      ""sql"": ""SELECT SUM(amount) as total FROM sales"",
      ""chartConfig"": null,
      ""settings"": {{}},
      ""row"": 0,
      ""column"": 0,
      ""width"": 1,
      ""height"": 1,
      ""refreshInterval"": ""5min""
    }},
    {{
      ""title"": ""Sales Trend"",
      ""widgetType"": ""chart"",
      ""sql"": ""SELECT date, SUM(amount) FROM sales GROUP BY date"",
      ""chartConfig"": {{
        ""chartType"": ""line"",
        ""xAxis"": ""date"",
        ""yAxis"": ""amount"",
        ""reason"": ""Shows trend over time""
      }},
      ""settings"": {{}},
      ""row"": 0,
      ""column"": 1,
      ""width"": 2,
      ""height"": 1
    }}
  ],
  ""layout"": ""grid"",
  ""theme"": ""light""
}}

Create a professional, actionable dashboard.";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<DashboardDefinition>(jsonMatch.Value, options);
                return result ?? new DashboardDefinition();
            }

            return new DashboardDefinition();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error generating dashboard: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// AI Data Quality Score: 0-100 rating for tables
    /// </summary>
    public async Task<DataQualityScore> CalculateDataQualityScoreAsync(
        string tableName,
        System.Data.DataTable sampleData,
        SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var dataSample = SerializeDataTableSample(sampleData, 30);

        var prompt = $@"You are a data quality expert. Calculate a comprehensive quality score (0-100) for this table.

Table: {tableName}

Sample data:
{dataSample}

Schema:
{SerializeSchema(schema)}

Analyze 5 dimensions:
1. Completeness (missing values)
2. Accuracy (valid values)
3. Consistency (format consistency)
4. Validity (business rules)
5. Uniqueness (duplicates)

Return JSON:
{{
  ""tableName"": ""{tableName}"",
  ""overallScore"": 87.5,
  ""dimensionScores"": {{
    ""completeness"": 95.0,
    ""accuracy"": 88.0,
    ""consistency"": 92.0,
    ""validity"": 85.0,
    ""uniqueness"": 78.0
  }},
  ""issues"": [
    {{
      ""category"": ""completeness"",
      ""description"": ""Email field has 12% null values"",
      ""severity"": ""medium"",
      ""impact"": 8.5,
      ""affectedColumns"": ""email""
    }}
  ],
  ""strengths"": [""All IDs are unique"", ""No invalid dates""],
  ""recommendations"": [
    {{
      ""action"": ""Add NOT NULL constraint to email field"",
      ""expectedImprovement"": ""+5 points completeness"",
      ""effort"": ""low"",
      ""priority"": ""high"",
      ""sql"": ""ALTER TABLE {tableName} ALTER COLUMN email SET NOT NULL""
    }}
  ],
  ""grade"": ""B+"",
  ""calculatedAt"": ""{DateTime.Now:yyyy-MM-dd HH:mm:ss}""
}}

Be thorough and actionable. Grades: A+ (95-100), A (90-94), B (80-89), C (70-79), D (60-69), F (<60)";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<DataQualityScore>(jsonMatch.Value, options);
                return result ?? new DataQualityScore();
            }

            return new DataQualityScore();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error calculating quality score: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Business Impact Analysis: Predict impact of schema changes
    /// </summary>
    public async Task<BusinessImpactAnalysis> AnalyzeSchemaChangeImpactAsync(
        string changeDescription,
        SchemaInfo schema,
        List<QueryHistoryEntry> historicalQueries)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var queryPatterns = string.Join("\n", historicalQueries.Take(20).Select(q =>
            $"- {q.NaturalLanguageQuery}\n  SQL: {q.GeneratedSql}"));

        var prompt = $@"You are a database impact analysis expert. Analyze the business impact of this schema change.

Proposed change: {changeDescription}

Current schema:
{SerializeSchema(schema)}

Historical query patterns:
{queryPatterns}

Analyze:
1. Which queries will break
2. Which objects (views, procedures) are affected
3. Business risks
4. Downtime estimate
5. Mitigation strategy
6. Rollback plan

Return comprehensive JSON:
{{
  ""changeDescription"": ""{changeDescription}"",
  ""impactLevel"": ""high"",
  ""affectedQueries"": [
    {{
      ""queryName"": ""Monthly Revenue Report"",
      ""sql"": ""SELECT revenue FROM sales"",
      ""impactType"": ""breaking"",
      ""description"": ""Column 'revenue' will be renamed"",
      ""suggestedFix"": ""UPDATE to use new column name""
    }}
  ],
  ""affectedObjects"": [
    {{
      ""objectType"": ""view"",
      ""objectName"": ""vw_sales_summary"",
      ""impactDescription"": ""View references renamed column"",
      ""requiresUpdate"": true
    }}
  ],
  ""risks"": [
    ""Reports will fail"",
    ""Dashboard will show errors"",
    ""ETL pipeline may break""
  ],
  ""mitigationSteps"": [
    ""Create column alias for backwards compatibility"",
    ""Update views before changing table"",
    ""Deploy during maintenance window""
  ],
  ""estimatedDowntimeMinutes"": 15,
  ""recommendedApproach"": ""Blue-green deployment with gradual migration"",
  ""rollbackPlan"": [
    ""Rename column back to original"",
    ""Restore views from backup"",
    ""Verify all queries work""
  ]
}}

Be conservative with risk assessment. Safety first.";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<BusinessImpactAnalysis>(jsonMatch.Value, options);
                return result ?? new BusinessImpactAnalysis();
            }

            return new BusinessImpactAnalysis();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error analyzing business impact: {ex.Message}", ex);
        }
    }

    private string SerializeDataTableSample(System.Data.DataTable data, int maxRows)
    {
        var sb = new StringBuilder();

        // Column headers
        sb.AppendLine(string.Join(" | ", data.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName)));
        sb.AppendLine(new string('-', 80));

        // Sample rows
        var rowsToShow = Math.Min(maxRows, data.Rows.Count);
        for (int i = 0; i < rowsToShow; i++)
        {
            var row = data.Rows[i];
            var values = row.ItemArray.Select(v => v?.ToString() ?? "[NULL]");
            sb.AppendLine(string.Join(" | ", values));
        }

        if (data.Rows.Count > maxRows)
        {
            sb.AppendLine($"... ({data.Rows.Count - maxRows} more rows)");
        }

        return sb.ToString();
    }

    private OpenAIResponse ParseOpenAIResponse(string content)
    {
        try
        {
            // Try to extract JSON (might be surrounded by markdown code blocks)
            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\{[\s\S]*\}",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var jsonContent = jsonMatch.Value;
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var response = JsonSerializer.Deserialize<OpenAIResponse>(jsonContent, options);
                if (response != null)
                {
                    return response;
                }
            }

            // Fallback: Try to extract SQL directly
            var sqlMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"```sql\s*(.*?)\s*```",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            if (sqlMatch.Success)
            {
                return new OpenAIResponse
                {
                    Sql = sqlMatch.Groups[1].Value.Trim(),
                    Explanation = "SQL was extracted, but no structured response received."
                };
            }

            // If everything fails, return raw content
            return new OpenAIResponse
            {
                Sql = content,
                Explanation = "Could not parse response - raw output"
            };
        }
        catch
        {
            return new OpenAIResponse
            {
                Sql = content,
                Explanation = "Error parsing OpenAI response"
            };
        }
    }

    /// <summary>
    /// Voice-to-SQL: Transcribe audio to text and generate SQL
    /// </summary>
    public async Task<VoiceTranscription> TranscribeAudioAsync(Stream audioStream, string fileName = "audio.wav")
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new AudioClient("whisper-1", _apiKey);

        try
        {
            var transcription = await client.TranscribeAudioAsync(audioStream, fileName);

            return new VoiceTranscription
            {
                TranscribedText = transcription.Value.Text,
                Language = transcription.Value.Language ?? "unknown",
                Confidence = 0.95, // Whisper doesn't provide confidence, using default
                DurationSeconds = transcription.Value.Duration?.TotalSeconds ?? 0
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error transcribing audio: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// AI Query Cost Estimator: Predicts cost and resource usage
    /// </summary>
    public async Task<QueryCostEstimate> EstimateQueryCostAsync(
        string sql,
        SchemaInfo schema,
        string cloudProvider = "azure") // azure|aws|on-premise
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var prompt = $@"You are a database performance and cost expert. Estimate the cost and resource usage for this SQL query.

SQL Query:
{sql}

Schema:
{SerializeSchema(schema)}

Cloud Provider: {cloudProvider}

Analyze:
1. Estimated execution time
2. Cloud cost (DTUs for Azure, RCUs for AWS, or processing cost)
3. Resource usage (CPU, Memory, I/O)
4. Cost optimization tips

Return JSON:
{{
  ""estimatedExecutionTimeSeconds"": 2.5,
  ""executionTimeCategory"": ""fast"",
  ""estimatedCloudCostUsd"": 0.0023,
  ""resourceUsage"": {{
    ""cpuUsage"": ""medium"",
    ""memoryUsage"": ""low"",
    ""ioUsage"": ""medium"",
    ""estimatedRowsScanned"": 50000,
    ""estimatedDataSizeMb"": 12.5
  }},
  ""costWarnings"": [""Large table scan - consider adding index""],
  ""optimizationTips"": [
    {{
      ""tip"": ""Add index on CustomerID"",
      ""potentialSavingPercent"": 45.0,
      ""effort"": ""low""
    }}
  ],
  ""alternativeQuery"": ""Optimized SQL if applicable"",
  ""alternativeCostSavingPercent"": 45.0,
  ""confidence"": 0.85
}}

Categories: instant (<0.5s), fast (<2s), moderate (<10s), slow (<30s), very_slow (>30s)
Cloud Cost Estimation: Consider table sizes, indexes, join complexity.";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<QueryCostEstimate>(jsonMatch.Value, options);
                return result ?? new QueryCostEstimate();
            }

            return new QueryCostEstimate();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error estimating query cost: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// AI Correlation Finder: Discovers hidden correlations in data
    /// </summary>
    public async Task<CorrelationAnalysis> FindCorrelationsAsync(
        System.Data.DataTable data,
        string sql)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var dataSample = SerializeDataTableSample(data, 30);

        var prompt = $@"You are a data scientist specialized in correlation analysis. Analyze this data for correlations.

Query:
{sql}

Data sample:
{dataSample}

Analyze:
1. Find correlations between numeric columns
2. Identify strong/weak relationships
3. Provide business insights
4. Warn about correlation vs causation

Return JSON:
{{
  ""correlations"": [
    {{
      ""column1"": ""Revenue"",
      ""column2"": ""CustomerSatisfaction"",
      ""correlationCoefficient"": 0.87,
      ""correlationType"": ""pearson"",
      ""strength"": ""strong"",
      ""direction"": ""positive"",
      ""pValue"": 0.002,
      ""isStatisticallySignificant"": true,
      ""interpretation"": ""Higher customer satisfaction is strongly associated with higher revenue""
    }}
  ],
  ""summary"": ""Found 3 significant correlations in the data"",
  ""insights"": [
    {{
      ""insight"": ""Customers with high satisfaction scores generate 87% more revenue"",
      ""actionableRecommendation"": ""Focus on customer satisfaction initiatives"",
      ""impact"": ""high""
    }}
  ],
  ""correlationMatrixCsv"": ""Column1,Column2,Coefficient\nRevenue,Satisfaction,0.87"",
  ""causationWarnings"": [""Correlation does not imply causation - further investigation needed""]
}}

Strength categories: very_strong (>0.9), strong (0.7-0.9), moderate (0.4-0.7), weak (0.2-0.4), very_weak (<0.2)";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<CorrelationAnalysis>(jsonMatch.Value, options);
                return result ?? new CorrelationAnalysis();
            }

            return new CorrelationAnalysis();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error finding correlations: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// AI Statistical Test Recommender: Recommends appropriate statistical tests
    /// </summary>
    public async Task<StatisticalTestRecommendation> RecommendStatisticalTestsAsync(
        System.Data.DataTable data,
        string researchQuestion)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var dataSample = SerializeDataTableSample(data, 20);

        var prompt = $@"You are a statistical consultant. Recommend appropriate statistical tests for this data.

Research Question: {researchQuestion}

Data sample:
{dataSample}

Analyze:
1. Data types (nominal, ordinal, interval, ratio)
2. Distribution shapes
3. Sample size
4. Recommend appropriate tests (t-test, ANOVA, chi-square, Mann-Whitney, etc.)

Return JSON:
{{
  ""dataInfo"": {{
    ""columnDataTypes"": {{""Age"": ""ratio"", ""Gender"": ""nominal""}},
    ""distributionShapes"": {{""Age"": ""normal"", ""Income"": ""skewed""}},
    ""sampleSize"": 150,
    ""numericColumnCount"": 3,
    ""categoricalColumnCount"": 2,
    ""hasPairedData"": false
  }},
  ""recommendedTests"": [
    {{
      ""testName"": ""Independent Samples t-test"",
      ""testType"": ""parametric"",
      ""purpose"": ""Compare means of two groups"",
      ""whenToUse"": [""Comparing two independent groups"", ""Continuous outcome variable"", ""Normal distribution""],
      ""assumptions"": [""Normality"", ""Homogeneity of variance"", ""Independence""],
      ""interpretation"": ""If p<0.05, there is a statistically significant difference between groups"",
      ""suitabilityScore"": 0.92
    }}
  ],
  ""primaryRecommendation"": ""Independent Samples t-test is most suitable"",
  ""assumptions"": [""Check normality with Shapiro-Wilk test"", ""Check variance with Levene's test""],
  ""guidance"": ""Your data appears normally distributed with two independent groups, making t-test ideal""
}}";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<StatisticalTestRecommendation>(jsonMatch.Value, options);
                return result ?? new StatisticalTestRecommendation();
            }

            return new StatisticalTestRecommendation();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error recommending statistical tests: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// AI Data Storytelling: Creates narrative story from data
    /// </summary>
    public async Task<DataStory> GenerateDataStoryAsync(
        System.Data.DataTable data,
        string sql,
        string naturalLanguageQuery,
        string tone = "professional") // professional|casual|technical
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var dataSample = SerializeDataTableSample(data, 50);

        var prompt = $@"You are a data storyteller. Transform this data into a compelling narrative story.

Original Question: {naturalLanguageQuery}

Query:
{sql}

Data results:
{dataSample}

Tone: {tone}

Create a story with:
1. Title (catchy, informative)
2. Executive Summary (2-3 sentences)
3. 3-5 Chapters (each with title and content paragraph)
4. Conclusion
5. Key Takeaways

Use storytelling elements:
- Characters (""Top Performers"", ""Outliers"", ""The Majority"")
- Plot (""In January... but then..."")
- Conflict (""The challenge was..."")
- Resolution (""By addressing X, we achieved Y"")

Return JSON:
{{
  ""title"": ""The Tale of Rising Revenue: A Q1 Success Story"",
  ""executiveSummary"": ""Our Q1 data reveals a remarkable 34% revenue increase driven primarily by new customer acquisition in the enterprise segment."",
  ""chapters"": [
    {{
      ""chapterTitle"": ""The Setting: January's Slow Start"",
      ""content"": ""January began with uncertainty. Revenue stood at $1.2M, slightly below expectations. The sales team faced headwinds from economic concerns, and customer acquisition had slowed to a trickle. However, beneath these surface challenges, a transformation was brewing."",
      ""keyFigures"": [""The Hesitant Customers"", ""The Determined Sales Team""],
      ""supportingData"": [""January Revenue: $1.2M"", ""New Customers: 23""]
    }}
  ],
  ""conclusion"": ""This quarter's story demonstrates the power of strategic focus and customer-centric innovation."",
  ""keyTakeaways"": [
    ""Enterprise segment is our growth driver (67% of new revenue)"",
    ""Customer satisfaction scores directly correlate with renewal rates (r=0.89)"",
    ""March represents an inflection point - strategies should continue""
  ],
  ""tone"": ""{tone}""
}}

Write in engaging, narrative style. Use concrete numbers. Make it memorable.";

        try
        {
            var response = await client.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content, @"\{[\s\S]*\}", System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<DataStory>(jsonMatch.Value, options);
                return result ?? new DataStory();
            }

            return new DataStory();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error generating data story: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Feature #25: AI Auto-Scheduler - Detects recurring query patterns
    /// </summary>
    public async Task<List<AutoSchedulerRecommendation>> DetectRecurringPatternsAsync(
        List<QueryHistoryEntry> historyEntries,
        int minOccurrences = 3)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        // Group queries by similarity
        var queryPatterns = string.Join("\n", historyEntries.Take(100).Select((e, i) =>
            $"{i+1}. [{e.ExecutedAt:yyyy-MM-dd HH:mm}] {e.NaturalLanguageQuery}"));

        var prompt = $@"You are an automation expert. Analyze these query execution patterns and identify recurring queries that should be automated.

Query History:
{queryPatterns}

Identify queries that:
1. Are executed repeatedly (at least {minOccurrences} times)
2. Follow a time pattern (daily, weekly, monthly, specific days/times)
3. Would benefit from automation

For each recurring pattern, provide:
- Pattern description
- Natural language query text (generalized)
- Recurrence pattern in human language
- Suggested cron schedule
- Confidence score (0.0-1.0)
- Reasoning
- Estimated time savings

Return JSON array:
[{{
  ""queryPattern"": ""Weekly sales report"",
  ""naturalLanguageQuery"": ""Show total sales for last week"",
  ""recurrencePattern"": ""Every Monday at 9:00 AM"",
  ""suggestedSchedule"": ""0 9 * * 1"",
  ""confidence"": 0.95,
  ""reasoning"": ""Executed every Monday morning for past 8 weeks"",
  ""occurrenceCount"": 8,
  ""estimatedTimeSaving"": ""2 hours per week"",
  ""priority"": ""High""
}}]";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an AI automation expert specializing in workflow optimization."),
            new UserChatMessage(prompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\[[\s\S]*\]",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var recommendations = JsonSerializer.Deserialize<List<AutoSchedulerRecommendation>>(jsonMatch.Value, options);
                return recommendations ?? new List<AutoSchedulerRecommendation>();
            }

            return new List<AutoSchedulerRecommendation>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error detecting recurring patterns: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Feature #30: AI What-If Simulator - Runs scenario simulation
    /// </summary>
    public async Task<SimulationResult> RunSimulationAsync(
        SimulationScenario scenario,
        System.Data.DataTable baselineData,
        SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var sampleData = SerializeDataTableSample(baselineData, 20);
        var paramChanges = string.Join("\n", scenario.Parameters.Select(p =>
            $"- {p.Name}: {p.BaseValue} → {p.SimulatedValue} ({p.ChangeDescription})"));

        var prompt = $@"You are a business analyst expert. Simulate the impact of these changes on the data.

Base Query: {scenario.BaseQuery}

Current Data Sample:
{sampleData}

Proposed Changes:
{paramChanges}

Analyze:
1. How will each KPI change?
2. What is the business impact?
3. What are the key findings?
4. What is the overall impact score (0-100)?
5. What is the risk level?

Return JSON:
{{
  ""summary"": ""Overall impact summary"",
  ""businessImpact"": ""Business implications"",
  ""keyFindings"": [""Finding 1"", ""Finding 2""],
  ""overallImpactScore"": 75,
  ""impactLevel"": ""High"",
  ""kpiComparisons"": [
    {{
      ""kpiName"": ""Total Revenue"",
      ""baselineValue"": ""$1M"",
      ""simulatedValue"": ""$1.1M"",
      ""changePercentage"": 10.0,
      ""changeDescription"": ""Increase of $100K"",
      ""trend"": ""Up"",
      ""interpretation"": ""Positive impact""
    }}
  ]
}}";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an AI business analyst specializing in scenario planning."),
            new UserChatMessage(prompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\{[\s\S]*\}",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<SimulationResult>(jsonMatch.Value, options);
                if (result != null)
                {
                    result.ScenarioId = scenario.Id;
                    result.ScenarioName = scenario.Name;
                    result.BaselineData = baselineData;
                    return result;
                }
            }

            return new SimulationResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error running simulation: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Feature #32: AI Root Cause Analyzer - Analyzes anomalies and finds root causes
    /// </summary>
    public async Task<RootCauseAnalysis> AnalyzeRootCauseAsync(
        string query,
        System.Data.DataTable data,
        string anomalyDescription,
        SchemaInfo schema)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var sampleData = SerializeDataTableSample(data, 50);
        var schemaInfo = SerializeSchema(schema);

        var prompt = $@"You are a data detective specializing in root cause analysis. Find the root cause of this anomaly.

Query: {query}

Anomaly: {anomalyDescription}

Data Sample:
{sampleData}

Schema:
{schemaInfo}

Perform multi-dimensional analysis:
1. What is the PRIMARY root cause?
2. What are contributing factors (with impact scores 0-100)?
3. Which dimensions are affected (time, geography, product, etc.)?
4. What drill-down queries would help investigate further?
5. What is the executive summary?

Return JSON:
{{
  ""primaryRootCause"": ""Main cause explanation"",
  ""executiveSummary"": ""2-3 sentence summary for executives"",
  ""confidenceScore"": 0.85,
  ""keyFindings"": [""Finding 1"", ""Finding 2""],
  ""actionPlan"": ""Recommended actions"",
  ""contributingFactors"": [
    {{
      ""name"": ""Factor name"",
      ""description"": ""Detailed explanation"",
      ""impactScore"": 85,
      ""impact"": ""High"",
      ""evidence"": ""Supporting data"",
      ""affectedDimensions"": [""Time"", ""Geography""],
      ""recommendation"": ""Suggested action""
    }}
  ],
  ""recommendedDrillDowns"": [
    {{
      ""title"": ""Analyze by Region"",
      ""description"": ""Break down by geographic region"",
      ""naturalLanguageQuery"": ""Show the same data grouped by region"",
      ""dimension"": ""Geography"",
      ""relevanceScore"": 0.9,
      ""expectedInsight"": ""Identify which regions are affected""
    }}
  ]
}}";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an AI data detective specializing in root cause analysis."),
            new UserChatMessage(prompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\{[\s\S]*\}",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<RootCauseAnalysis>(jsonMatch.Value, options);
                if (result != null)
                {
                    result.Query = query;
                    result.AnomalyDescription = anomalyDescription;
                    return result;
                }
            }

            return new RootCauseAnalysis();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error analyzing root cause: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Feature #40: AI Compliance Copilot - Checks query for regulatory compliance
    /// </summary>
    public async Task<ComplianceReport> CheckComplianceAsync(
        string query,
        SchemaInfo schema,
        List<ComplianceFramework> frameworks)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key is not configured");
        }

        var client = new ChatClient(_model, _apiKey);

        var schemaInfo = SerializeSchema(schema);
        var frameworkList = string.Join(", ", frameworks.Select(f => f.ToString()));

        var prompt = $@"You are a compliance expert specializing in data protection regulations. Analyze this query for compliance violations.

Query: {query}

Schema:
{schemaInfo}

Regulations to check: {frameworkList}

Analyze:
1. Does this query access PII/sensitive data?
2. Are there GDPR/SOX/HIPAA violations?
3. What is the overall risk score (0-100)?
4. What actions are required for compliance?
5. Does this require approval, and at what level?

Return JSON:
{{
  ""overallRiskScore"": 75,
  ""status"": ""PartiallyCompliant"",
  ""executiveSummary"": ""Query accesses customer data without proper safeguards"",
  ""requiresApproval"": true,
  ""approvalLevel"": ""Director"",
  ""regulations"": [
    {{
      ""regulationName"": ""GDPR"",
      ""article"": ""Art. 32"",
      ""description"": ""Security of processing"",
      ""isCompliant"": false,
      ""status"": ""violation"",
      ""requirements"": [""Encryption"", ""Access control""],
      ""findings"": [""Personal data accessed without encryption""],
      ""recommendation"": ""Implement column-level encryption""
    }}
  ],
  ""violations"": [
    {{
      ""violationType"": ""Unencrypted PII Access"",
      ""regulation"": ""GDPR Art. 32"",
      ""description"": ""Query accesses email addresses without encryption"",
      ""severity"": ""High"",
      ""affectedData"": ""Email, Phone"",
      ""affectedColumns"": [""Customers.Email"", ""Customers.Phone""],
      ""potentialImpact"": ""GDPR fine up to 4% of revenue"",
      ""requiredAction"": ""Add encryption or mask data"",
      ""penaltyRisk"": 50000
    }}
  ],
  ""warnings"": [
    {{
      ""warningType"": ""Broad data access"",
      ""description"": ""Query retrieves more columns than necessary"",
      ""recommendation"": ""Use specific column names instead of SELECT *"",
      ""preventionTip"": ""Principle of data minimization""
    }}
  ],
  ""remediation"": {{
    ""priority"": ""High"",
    ""estimatedEffort"": 2,
    ""steps"": [
      {{
        ""order"": 1,
        ""action"": ""Add WHERE clause to limit data"",
        ""description"": ""Restrict to necessary records only"",
        ""sqlFix"": ""WHERE created_date > DATEADD(month, -1, GETDATE())"",
        ""estimatedMinutes"": 30,
        ""requiresApproval"": false
      }}
    ]
  }}
}}";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an AI compliance expert specializing in GDPR, SOX, HIPAA, and data protection."),
            new UserChatMessage(prompt)
        };

        try
        {
            var response = await client.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                content,
                @"\{[\s\S]*\}",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            if (jsonMatch.Success)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ComplianceReport>(jsonMatch.Value, options);
                if (result != null)
                {
                    result.Query = query;
                    return result;
                }
            }

            return new ComplianceReport();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking compliance: {ex.Message}", ex);
        }
    }
}

