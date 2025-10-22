namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI-powered predictive trends and forecasting
/// </summary>
public class PredictiveTrendAnalysis
{
    public string AnalyzedColumn { get; set; } = string.Empty;
    public TrendDirection TrendDirection { get; set; } = new();
    public List<ForecastPoint> Forecast { get; set; } = new();
    public int ForecastDays { get; set; }
    public string SeasonalityPattern { get; set; } = string.Empty; // none|daily|weekly|monthly|yearly
    public List<PredictedAnomaly> PredictedAnomalies { get; set; } = new();
    public double ForecastConfidence { get; set; } // 0.0-1.0
    public string Summary { get; set; } = string.Empty;
    public List<string> Insights { get; set; } = new();
}

/// <summary>
/// Trend direction analysis
/// </summary>
public class TrendDirection
{
    public string Direction { get; set; } = string.Empty; // upward|downward|stable|volatile
    public double Slope { get; set; } // Rate of change
    public double ChangePercentage { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}

/// <summary>
/// Individual forecast point
/// </summary>
public class ForecastPoint
{
    public DateTime Date { get; set; }
    public double PredictedValue { get; set; }
    public double LowerBound { get; set; } // 95% confidence interval
    public double UpperBound { get; set; } // 95% confidence interval
    public double Confidence { get; set; }
}

/// <summary>
/// Predicted future anomaly
/// </summary>
public class PredictedAnomaly
{
    public DateTime ExpectedDate { get; set; }
    public string AnomalyType { get; set; } = string.Empty; // spike|drop|outlier
    public string Reason { get; set; } = string.Empty;
    public double Probability { get; set; } // 0.0-1.0
    public string RecommendedAction { get; set; } = string.Empty;
}
