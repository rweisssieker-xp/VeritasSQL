namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents AI analysis of SQL errors with fix suggestions
/// </summary>
public class SqlErrorAnalysis
{
    public string ErrorType { get; set; } = string.Empty; // syntax|permission|schema|logic|other
    public string Explanation { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public string FixedSql { get; set; } = string.Empty;
    public List<string> LearningPoints { get; set; } = new();
    public string Severity { get; set; } = "medium"; // critical|high|medium|low
}
