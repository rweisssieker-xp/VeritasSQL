namespace VeritasSQL.Core.Models;

/// <summary>
/// Result of AI root cause analysis
/// </summary>
public class RootCauseAnalysis
{
    public string Query { get; set; } = string.Empty;
    public string AnomalyDescription { get; set; } = string.Empty;
    public string PrimaryRootCause { get; set; } = string.Empty;
    public List<CauseFactor> ContributingFactors { get; set; } = new();
    public List<DrillDownSuggestion> RecommendedDrillDowns { get; set; } = new();
    public string ExecutiveSummary { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; } // 0.0-1.0
    public DateTime AnalyzedAt { get; set; } = DateTime.Now;
    public List<string> KeyFindings { get; set; } = new();
    public string ActionPlan { get; set; } = string.Empty;
}

/// <summary>
/// A contributing factor to the root cause
/// </summary>
public class CauseFactor
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double ImpactScore { get; set; } // 0-100
    public ImpactLevel Impact { get; set; }
    public string Evidence { get; set; } = string.Empty;
    public List<string> AffectedDimensions { get; set; } = new(); // Time, Geography, Product, etc.
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Suggested drill-down query to investigate further
/// </summary>
public class DrillDownSuggestion
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string GeneratedSql { get; set; } = string.Empty;
    public string Dimension { get; set; } = string.Empty; // What dimension to slice by
    public double RelevanceScore { get; set; } // 0.0-1.0
    public string ExpectedInsight { get; set; } = string.Empty;
}

/// <summary>
/// Represents a path through drill-down analysis
/// </summary>
public class DrillDownPath
{
    public List<DrillDownStep> Steps { get; set; } = new();
    public string CurrentLevel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool IssueResolved { get; set; }
}

public class DrillDownStep
{
    public int StepNumber { get; set; }
    public string Query { get; set; } = string.Empty;
    public string Finding { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
}
