namespace VeritasSQL.Core.Models;

public class DatabaseHealthReport
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int OverallHealthScore { get; set; } // 0-100
    public HealthStatus Status { get; set; }
    public List<HealthMetric> Metrics { get; set; } = new();
    public List<HealthAlert> Alerts { get; set; } = new();
    public List<HealthTrend> Trends { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public enum HealthStatus { Excellent, Good, Fair, Poor, Critical }

public class HealthMetric
{
    public string Name { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double Threshold { get; set; }
    public string Unit { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
}

public class HealthAlert
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // critical|high|medium|low
    public DateTime DetectedAt { get; set; } = DateTime.Now;
    public string Recommendation { get; set; } = string.Empty;
}

public class HealthTrend
{
    public string MetricName { get; set; } = string.Empty;
    public string TrendDirection { get; set; } = string.Empty; // improving|degrading|stable
    public double ChangePercentage { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}
