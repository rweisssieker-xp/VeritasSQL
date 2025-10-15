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

