namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI recommendations for data visualization
/// </summary>
public class VisualizationRecommendations
{
    public ChartRecommendation? PrimaryRecommendation { get; set; }
    public List<ChartRecommendation> AlternativeRecommendations { get; set; } = new();
    public string Insights { get; set; } = string.Empty;
}

/// <summary>
/// Individual chart recommendation
/// </summary>
public class ChartRecommendation
{
    public string ChartType { get; set; } = string.Empty; // bar|line|pie|scatter|table|heatmap|area
    public string Reason { get; set; } = string.Empty;
    public string? XAxis { get; set; }
    public string? YAxis { get; set; }
    public string? Configuration { get; set; }
    public string? UseCase { get; set; }
}
