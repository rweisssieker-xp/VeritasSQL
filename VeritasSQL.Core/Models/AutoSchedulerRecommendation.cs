namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents an AI recommendation for automating recurring queries
/// </summary>
public class AutoSchedulerRecommendation
{
    public string QueryPattern { get; set; } = string.Empty;
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string GeneratedSql { get; set; } = string.Empty;
    public string RecurrencePattern { get; set; } = string.Empty; // "Every Monday 9am", "Daily at 8am", etc.
    public double Confidence { get; set; } // 0.0-1.0
    public string Reasoning { get; set; } = string.Empty;
    public int OccurrenceCount { get; set; } // How many times this pattern was detected
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
    public List<DateTime> DetectedExecutionTimes { get; set; } = new();
    public string SuggestedSchedule { get; set; } = string.Empty; // Cron expression
    public List<string> AffectedStakeholders { get; set; } = new(); // Who runs this query
    public string EstimatedTimeSaving { get; set; } = string.Empty; // "2 hours/week"
    public AutomationPriority Priority { get; set; }
}

public enum AutomationPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Represents a configured automated job
/// </summary>
public class AutomationJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string GeneratedSql { get; set; } = string.Empty;
    public string ConnectionProfileId { get; set; } = string.Empty;
    public string CronSchedule { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastExecutedAt { get; set; }
    public DateTime? NextExecutionAt { get; set; }
    public int ExecutionCount { get; set; }
    public AutomationAction Action { get; set; }
    public string ExportPath { get; set; } = string.Empty;
    public ExportFormat ExportFormat { get; set; }
    public List<string> EmailRecipients { get; set; } = new();
    public bool SendEmailOnSuccess { get; set; }
    public bool SendEmailOnFailure { get; set; } = true;
    public int RetryCount { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 300;
}

public enum AutomationAction
{
    ExecuteOnly,
    ExportToCsv,
    ExportToExcel,
    EmailResults,
    SaveToDatabase
}

public enum ExportFormat
{
    Csv,
    Excel,
    Json,
    Xml
}

/// <summary>
/// Represents the execution history of an automated job
/// </summary>
public class JobExecution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JobId { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.Now;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int RowCount { get; set; }
    public double ExecutionTimeMs { get; set; }
    public string? ExportedFilePath { get; set; }
    public bool EmailSent { get; set; }
    public List<string> EmailRecipients { get; set; } = new();
}
