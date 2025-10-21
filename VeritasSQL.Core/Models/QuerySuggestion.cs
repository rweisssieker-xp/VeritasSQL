namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents an AI-generated query suggestion
/// </summary>
public class QuerySuggestion
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string Complexity { get; set; } = "low"; // low|medium|high
    public string Category { get; set; } = string.Empty; // analytics|reporting|data_quality|relationships
}
