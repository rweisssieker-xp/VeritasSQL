namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents the response from OpenAI for SQL generation
/// </summary>
public class OpenAIResponse
{
    public string Sql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<string> TablesUsed { get; set; } = new();
    public string EstimatedCost { get; set; } = "Unknown";
}

