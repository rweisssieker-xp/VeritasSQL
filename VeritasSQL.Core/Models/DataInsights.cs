namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI-powered insights about query results
/// </summary>
public class DataInsights
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Insights { get; set; } = new();
    public List<string> DataQualityIssues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DataStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Statistical information about the data
/// </summary>
public class DataStatistics
{
    public double NullPercentage { get; set; }
    public int UniqueValues { get; set; }
    public int PotentialDuplicates { get; set; }
}
