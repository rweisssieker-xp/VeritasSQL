namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents automated dashboard generation
/// </summary>
public class DashboardDefinition
{
    public string DashboardId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DashboardWidget> Widgets { get; set; } = new();
    public string Layout { get; set; } = "grid"; // grid|flow|tabs
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public string Theme { get; set; } = "light"; // light|dark|auto
}

/// <summary>
/// Individual dashboard widget
/// </summary>
public class DashboardWidget
{
    public string WidgetId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty; // chart|kpi|table|text
    public string Sql { get; set; } = string.Empty;
    public ChartRecommendation? ChartConfig { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
    public int Row { get; set; }
    public int Column { get; set; }
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;
    public string RefreshInterval { get; set; } = "manual"; // manual|1min|5min|15min
}

/// <summary>
/// Represents AI-calculated data quality score
/// </summary>
public class DataQualityScore
{
    public string TableName { get; set; } = string.Empty;
    public double OverallScore { get; set; } // 0-100
    public Dictionary<string, double> DimensionScores { get; set; } = new(); // completeness, accuracy, etc.
    public List<QualityIssue> Issues { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public List<QualityRecommendation> Recommendations { get; set; } = new();
    public string Grade { get; set; } = "C"; // A+|A|B|C|D|F
    public DateTime CalculatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Quality issue found
/// </summary>
public class QualityIssue
{
    public string Category { get; set; } = string.Empty; // completeness|consistency|accuracy|validity|uniqueness
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "medium"; // critical|high|medium|low
    public double Impact { get; set; } // 0-100
    public string AffectedColumns { get; set; } = string.Empty;
}

/// <summary>
/// Quality improvement recommendation
/// </summary>
public class QualityRecommendation
{
    public string Action { get; set; } = string.Empty;
    public string ExpectedImprovement { get; set; } = string.Empty;
    public string Effort { get; set; } = "medium"; // low|medium|high
    public string Priority { get; set; } = "medium"; // critical|high|medium|low
    public string? Sql { get; set; }
}

/// <summary>
/// Represents business impact analysis for schema changes
/// </summary>
public class BusinessImpactAnalysis
{
    public string ChangeDescription { get; set; } = string.Empty;
    public string ImpactLevel { get; set; } = "medium"; // critical|high|medium|low
    public List<ImpactedQuery> AffectedQueries { get; set; } = new();
    public List<ImpactedObject> AffectedObjects { get; set; } = new();
    public List<string> Risks { get; set; } = new();
    public List<string> MitigationSteps { get; set; } = new();
    public int EstimatedDowntimeMinutes { get; set; }
    public string RecommendedApproach { get; set; } = string.Empty;
    public List<string> RollbackPlan { get; set; } = new();
}

/// <summary>
/// Impacted query
/// </summary>
public class ImpactedQuery
{
    public string QueryName { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
    public string ImpactType { get; set; } = string.Empty; // breaking|warning|informational
    public string Description { get; set; } = string.Empty;
    public string? SuggestedFix { get; set; }
}

/// <summary>
/// Impacted database object
/// </summary>
public class ImpactedObject
{
    public string ObjectType { get; set; } = string.Empty; // table|view|procedure|function|trigger
    public string ObjectName { get; set; } = string.Empty;
    public string ImpactDescription { get; set; } = string.Empty;
    public bool RequiresUpdate { get; set; }
}
