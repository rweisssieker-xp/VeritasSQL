using System.Data;

namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents the result of a SQL query
/// </summary>
public class QueryResult
{
    public bool Success { get; set; }
    public DataTable? Data { get; set; }
    public int RowCount { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string? ErrorMessage { get; set; }
    public string SqlQuery { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates if this is a preview result (limited rows)
    /// </summary>
    public bool IsPreview { get; set; }
    
    /// <summary>
    /// The row limit used for preview (e.g., 5)
    /// </summary>
    public int PreviewRowLimit { get; set; }
}

/// <summary>
/// Represents a validation result for SQL
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
    public string? ModifiedSql { get; set; }

    public bool HasErrors => Issues.Any(i => i.Severity == IssueSeverity.Error);
    public bool HasWarnings => Issues.Any(i => i.Severity == IssueSeverity.Warning);
}

public class ValidationIssue
{
    public IssueSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Suggestion { get; set; }
}

public enum IssueSeverity
{
    Info,
    Warning,
    Error
}

