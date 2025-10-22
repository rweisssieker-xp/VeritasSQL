namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI-predicted query cost and resource usage
/// </summary>
public class QueryCostEstimate
{
    public double EstimatedExecutionTimeSeconds { get; set; }
    public string ExecutionTimeCategory { get; set; } = string.Empty; // instant|fast|moderate|slow|very_slow
    public double EstimatedCloudCostUsd { get; set; }
    public ResourceUsageEstimate ResourceUsage { get; set; } = new();
    public List<string> CostWarnings { get; set; } = new();
    public List<CostOptimizationTip> OptimizationTips { get; set; } = new();
    public string AlternativeQuery { get; set; } = string.Empty;
    public double AlternativeCostSavingPercent { get; set; }
    public double Confidence { get; set; } // 0.0-1.0
}

/// <summary>
/// Resource usage estimates
/// </summary>
public class ResourceUsageEstimate
{
    public string CpuUsage { get; set; } = "low"; // low|medium|high|very_high
    public string MemoryUsage { get; set; } = "low";
    public string IoUsage { get; set; } = "low";
    public int EstimatedRowsScanned { get; set; }
    public double EstimatedDataSizeMb { get; set; }
}

/// <summary>
/// Cost optimization tip
/// </summary>
public class CostOptimizationTip
{
    public string Tip { get; set; } = string.Empty;
    public double PotentialSavingPercent { get; set; }
    public string Effort { get; set; } = "low"; // low|medium|high
}
