namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI-powered index recommendations
/// </summary>
public class IndexRecommendationResult
{
    public List<IndexRecommendation> Recommendations { get; set; } = new();
    public List<SlowQueryAnalysis> SlowQueries { get; set; } = new();
    public IndexHealthAssessment HealthAssessment { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public double EstimatedPerformanceGain { get; set; } // Overall percentage
}

/// <summary>
/// Individual index recommendation
/// </summary>
public class IndexRecommendation
{
    public string TableName { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public string IndexType { get; set; } = string.Empty; // nonclustered|clustered|covering|unique
    public string CreateIndexSql { get; set; } = string.Empty;
    public int AffectedQueriesCount { get; set; }
    public double EstimatedImprovementPercent { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public string Priority { get; set; } = "medium"; // critical|high|medium|low
    public bool IsCoveringIndex { get; set; }
    public List<string> IncludedColumns { get; set; } = new();
}

/// <summary>
/// Slow query analysis
/// </summary>
public class SlowQueryAnalysis
{
    public string QueryPattern { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
    public double AverageExecutionTimeMs { get; set; }
    public int ExecutionCount { get; set; }
    public List<string> MissingIndexes { get; set; } = new();
    public string BottleneckType { get; set; } = string.Empty; // table_scan|index_scan|key_lookup|sort
    public string RecommendedFix { get; set; } = string.Empty;
}

/// <summary>
/// Index health assessment
/// </summary>
public class IndexHealthAssessment
{
    public int TotalIndexes { get; set; }
    public int UnusedIndexes { get; set; }
    public int DuplicateIndexes { get; set; }
    public int FragmentedIndexes { get; set; }
    public List<string> MaintenanceRecommendations { get; set; } = new();
    public string OverallHealth { get; set; } = "good"; // excellent|good|fair|poor
}
