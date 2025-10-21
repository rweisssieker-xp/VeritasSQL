using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using VeritasSQL.Core.Models;

namespace VeritasSQL.Core.Services;

/// <summary>
/// Communicates with OpenAI API for NL-to-SQL translation
/// </summary>
public class OpenAIService
{
    private readonly string? _apiKey;
    private readonly string _model;

    public OpenAIService(string? apiKey, string model = "gpt-4")
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
        sb.AppendLine("Generate only SELECT statements. No DML (INSERT, UPDATE, DELETE) or DDL (CREATE, DROP, ALTER).");
        sb.AppendLine("Rules:");
        sb.AppendLine("1. Use fully qualified object names (Schema.Table)");
        sb.AppendLine("2. Always add TOP N (default: TOP 100)");
        sb.AppendLine("3. Only use tables/views from the provided schema");
        sb.AppendLine("4. Use parameterized queries for filters");
        sb.AppendLine("5. No multiple statements (no semicolons)");
        sb.AppendLine("6. No dangerous functions (EXEC, xp_cmdshell, etc.)");
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
        sb.AppendLine("Respond in the following JSON format:");
        sb.AppendLine("{");
        sb.AppendLine("  \"sql\": \"SELECT ...\",");
        sb.AppendLine("  \"explanation\": \"Explanation in English\",");
        sb.AppendLine("  \"tables_used\": [\"Schema.Table1\", \"Schema.Table2\"],");
        sb.AppendLine("  \"estimated_cost\": \"low|medium|high\"");
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
}

