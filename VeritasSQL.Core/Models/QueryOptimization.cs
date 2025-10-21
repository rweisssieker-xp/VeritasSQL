namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI-powered query optimization suggestions
/// </summary>
public class QueryOptimization
{
    public string PerformanceRating { get; set; } = "unknown"; // excellent|good|fair|poor
    public string OptimizedSql { get; set; } = string.Empty;
    public List<OptimizationRecommendation> Recommendations { get; set; } = new();
    public string EstimatedImprovement { get; set; } = string.Empty;
}

/// <summary>
/// Individual optimization recommendation
/// </summary>
public class OptimizationRecommendation
{
    public string Type { get; set; } = string.Empty; // index|rewrite|join|general
    public string Priority { get; set; } = "medium"; // high|medium|low
    public string Issue { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}
