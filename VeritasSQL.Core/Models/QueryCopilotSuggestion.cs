namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents real-time AI query co-pilot suggestions
/// </summary>
public class QueryCopilotSuggestion
{
    public string CompletionText { get; set; } = string.Empty;
    public string FullSql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; } // 0.0 - 1.0
    public string Category { get; set; } = string.Empty; // completion|correction|enhancement
}

/// <summary>
/// Represents predictive next query suggestions
/// </summary>
public class PredictiveQuerySuggestion
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public double RelevanceScore { get; set; } // 0.0 - 1.0
    public string Category { get; set; } = string.Empty; // follow_up|related|drill_down|comparison
}

/// <summary>
/// Represents optimal JOIN path between tables
/// </summary>
public class JoinPathResult
{
    public string SourceTable { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public List<JoinStep> Path { get; set; } = new();
    public string CompleteSql { get; set; } = string.Empty;
    public int PathLength { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<string> AlternativePaths { get; set; } = new();
}

/// <summary>
/// Individual step in a JOIN path
/// </summary>
public class JoinStep
{
    public string FromTable { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string JoinType { get; set; } = "INNER"; // INNER|LEFT|RIGHT|FULL
    public string OnClause { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty; // one_to_many|many_to_one|one_to_one
}
