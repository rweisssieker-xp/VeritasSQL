namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents comprehensive AI data profiling with PII detection
/// </summary>
public class DataProfilingResult
{
    public string TableName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int TotalColumns { get; set; }
    public List<ColumnProfile> ColumnProfiles { get; set; } = new();
    public List<PiiDetection> PiiFindings { get; set; } = new();
    public DataQualitySummary QualitySummary { get; set; } = new();
    public List<string> ComplianceWarnings { get; set; } = new();
    public string OverallRisk { get; set; } = "low"; // critical|high|medium|low
}

/// <summary>
/// Profile for individual column
/// </summary>
public class ColumnProfile
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int DistinctValues { get; set; }
    public double NullPercentage { get; set; }
    public string MinValue { get; set; } = string.Empty;
    public string MaxValue { get; set; } = string.Empty;
    public string MostCommonValue { get; set; } = string.Empty;
    public List<string> DataPatterns { get; set; } = new();
    public bool IsPotentiallyPii { get; set; }
}

/// <summary>
/// PII detection result
/// </summary>
public class PiiDetection
{
    public string ColumnName { get; set; } = string.Empty;
    public string PiiType { get; set; } = string.Empty; // email|phone|ssn|credit_card|name|address|ip_address
    public double Confidence { get; set; } // 0.0 - 1.0
    public string Recommendation { get; set; } = string.Empty;
    public string GdprCategory { get; set; } = string.Empty; // personal_data|sensitive_data|special_category
    public List<string> SampleValues { get; set; } = new(); // redacted samples
}

/// <summary>
/// Data quality summary
/// </summary>
public class DataQualitySummary
{
    public double CompletenessScore { get; set; } // 0-100
    public double UniquenessScore { get; set; } // 0-100
    public double ConsistencyScore { get; set; } // 0-100
    public double ValidityScore { get; set; } // 0-100
    public List<string> QualityIssues { get; set; } = new();
}
