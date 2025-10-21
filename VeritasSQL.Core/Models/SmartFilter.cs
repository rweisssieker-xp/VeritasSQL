namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents an AI-suggested filter for queries
/// </summary>
public class SmartFilter
{
    public string Column { get; set; } = string.Empty;
    public string FilterType { get; set; } = "equals"; // equals|range|in|like|date_range
    public string SuggestedValue { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
