namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI-discovered correlations in data
/// </summary>
public class CorrelationAnalysis
{
    public List<ColumnCorrelation> Correlations { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public List<BusinessInsight> Insights { get; set; } = new();
    public string CorrelationMatrixCsv { get; set; } = string.Empty; // For export
    public List<string> CausationWarnings { get; set; } = new();
}

/// <summary>
/// Correlation between two columns
/// </summary>
public class ColumnCorrelation
{
    public string Column1 { get; set; } = string.Empty;
    public string Column2 { get; set; } = string.Empty;
    public double CorrelationCoefficient { get; set; } // -1.0 to 1.0
    public string CorrelationType { get; set; } = string.Empty; // pearson|spearman
    public string Strength { get; set; } = string.Empty; // very_strong|strong|moderate|weak|very_weak
    public string Direction { get; set; } = string.Empty; // positive|negative
    public double PValue { get; set; }
    public bool IsStatisticallySignificant { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}

/// <summary>
/// Business insight from correlation
/// </summary>
public class BusinessInsight
{
    public string Insight { get; set; } = string.Empty;
    public string ActionableRecommendation { get; set; } = string.Empty;
    public string Impact { get; set; } = "medium"; // low|medium|high
}
