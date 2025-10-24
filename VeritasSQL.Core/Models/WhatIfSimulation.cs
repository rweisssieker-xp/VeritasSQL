namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents a what-if scenario configuration
/// </summary>
public class SimulationScenario
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseQuery { get; set; } = string.Empty;
    public List<ScenarioParameter> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Parameter for a simulation scenario
/// </summary>
public class ScenarioParameter
{
    public string Name { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public ParameterType Type { get; set; }
    public string BaseValue { get; set; } = string.Empty;
    public string SimulatedValue { get; set; } = string.Empty;
    public double ChangePercentage { get; set; } // For numeric values
    public string ChangeDescription { get; set; } = string.Empty; // "Increase by 10%", etc.
}

public enum ParameterType
{
    Numeric,
    Percentage,
    Text,
    Date,
    Boolean
}

/// <summary>
/// Result of running a what-if simulation
/// </summary>
public class SimulationResult
{
    public string ScenarioId { get; set; } = string.Empty;
    public string ScenarioName { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.Now;
    public System.Data.DataTable BaselineData { get; set; } = new();
    public System.Data.DataTable SimulatedData { get; set; } = new();
    public List<KpiComparison> KpiComparisons { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public string BusinessImpact { get; set; } = string.Empty;
    public List<string> KeyFindings { get; set; } = new();
    public double OverallImpactScore { get; set; } // 0-100
    public ImpactLevel ImpactLevel { get; set; }
}

/// <summary>
/// Comparison of KPIs between baseline and simulated scenario
/// </summary>
public class KpiComparison
{
    public string KpiName { get; set; } = string.Empty;
    public string BaselineValue { get; set; } = string.Empty;
    public string SimulatedValue { get; set; } = string.Empty;
    public double ChangePercentage { get; set; }
    public string ChangeDescription { get; set; } = string.Empty;
    public TrendDirection Trend { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}

public enum TrendDirection
{
    Up,
    Down,
    Stable
}

public enum ImpactLevel
{
    Minimal,
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Comparison of multiple scenarios side-by-side
/// </summary>
public class ScenarioComparison
{
    public List<SimulationResult> Scenarios { get; set; } = new();
    public string BestScenario { get; set; } = string.Empty;
    public string WorstScenario { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public List<string> KeyInsights { get; set; } = new();
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<RiskFactor> Risks { get; set; } = new();
}

public class RiskFactor
{
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // low | medium | high
    public string Mitigation { get; set; } = string.Empty;
}
