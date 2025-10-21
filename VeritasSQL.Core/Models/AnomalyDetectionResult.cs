namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI-detected anomalies in query results
/// </summary>
public class AnomalyDetectionResult
{
    public bool HasAnomalies { get; set; }
    public int AnomalyCount { get; set; }
    public List<DataAnomaly> Anomalies { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Individual data anomaly
/// </summary>
public class DataAnomaly
{
    public string Type { get; set; } = string.Empty; // outlier|missing_data|suspicious_value|integrity_issue
    public string Description { get; set; } = string.Empty;
    public string AffectedRows { get; set; } = string.Empty;
    public string Severity { get; set; } = "medium"; // high|medium|low
    public string Recommendation { get; set; } = string.Empty;
}
