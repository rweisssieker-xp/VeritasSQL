namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI-recommended statistical tests for data
/// </summary>
public class StatisticalTestRecommendation
{
    public DataCharacteristics DataInfo { get; set; } = new();
    public List<RecommendedTest> RecommendedTests { get; set; } = new();
    public string PrimaryRecommendation { get; set; } = string.Empty;
    public List<string> Assumptions { get; set; } = new();
    public string Guidance { get; set; } = string.Empty;
}

/// <summary>
/// Data characteristics for test selection
/// </summary>
public class DataCharacteristics
{
    public Dictionary<string, string> ColumnDataTypes { get; set; } = new(); // column -> nominal|ordinal|interval|ratio
    public Dictionary<string, string> DistributionShapes { get; set; } = new(); // column -> normal|skewed|uniform|bimodal
    public int SampleSize { get; set; }
    public int NumericColumnCount { get; set; }
    public int CategoricalColumnCount { get; set; }
    public bool HasPairedData { get; set; }
}

/// <summary>
/// Recommended statistical test
/// </summary>
public class RecommendedTest
{
    public string TestName { get; set; } = string.Empty; // t-test|ANOVA|chi-square|mann-whitney|etc
    public string TestType { get; set; } = string.Empty; // parametric|non-parametric
    public string Purpose { get; set; } = string.Empty;
    public List<string> WhenToUse { get; set; } = new();
    public List<string> Assumptions { get; set; } = new();
    public string Interpretation { get; set; } = string.Empty;
    public double SuitabilityScore { get; set; } // 0.0-1.0
}

/// <summary>
/// Results of executed statistical test
/// </summary>
public class StatisticalTestResult
{
    public string TestName { get; set; } = string.Empty;
    public double TestStatistic { get; set; }
    public double PValue { get; set; }
    public string PValueInterpretation { get; set; } = string.Empty;
    public bool IsSignificant { get; set; }
    public double? EffectSize { get; set; }
    public string EffectSizeInterpretation { get; set; } = string.Empty;
    public string Conclusion { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}
