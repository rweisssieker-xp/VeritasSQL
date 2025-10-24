namespace VeritasSQL.Core.Models;

/// <summary>
/// Comprehensive compliance report
/// </summary>
public class ComplianceReport
{
    public string Query { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; } = DateTime.Now;
    public int OverallRiskScore { get; set; } // 0-100
    public ComplianceStatus Status { get; set; }
    public List<RegulationCheck> Regulations { get; set; } = new();
    public List<ComplianceViolation> Violations { get; set; } = new();
    public List<ComplianceWarning> Warnings { get; set; } = new();
    public RemediationPlan Remediation { get; set; } = new();
    public string ExecutiveSummary { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; }
    public string ApprovalLevel { get; set; } = string.Empty; // Manager | Director | C-Level
}

public enum ComplianceStatus
{
    Compliant,
    PartiallyCompliant,
    NonCompliant,
    RequiresReview
}

/// <summary>
/// Check against a specific regulation
/// </summary>
public class RegulationCheck
{
    public string RegulationName { get; set; } = string.Empty; // GDPR, SOX, HIPAA, etc.
    public string Article { get; set; } = string.Empty; // Art. 32 GDPR, etc.
    public string Description { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty; // compliant | warning | violation
    public List<string> Requirements { get; set; } = new();
    public List<string> Findings { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// A compliance violation that must be addressed
/// </summary>
public class ComplianceViolation
{
    public string ViolationType { get; set; } = string.Empty;
    public string Regulation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ComplianceSeverity Severity { get; set; }
    public string AffectedData { get; set; } = string.Empty;
    public List<string> AffectedColumns { get; set; } = new();
    public string PotentialImpact { get; set; } = string.Empty;
    public string RequiredAction { get; set; } = string.Empty;
    public int PenaltyRisk { get; set; } // Potential fine in USD
}

public enum ComplianceSeverity
{
    Critical,
    High,
    Medium,
    Low
}

/// <summary>
/// A compliance warning (not yet a violation)
/// </summary>
public class ComplianceWarning
{
    public string WarningType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string PreventionTip { get; set; } = string.Empty;
}

/// <summary>
/// Plan to remediate compliance issues
/// </summary>
public class RemediationPlan
{
    public List<RemediationStep> Steps { get; set; } = new();
    public int EstimatedEffort { get; set; } // Hours
    public string Priority { get; set; } = string.Empty;
    public DateTime SuggestedDeadline { get; set; }
    public List<string> RequiredApprovals { get; set; } = new();
    public string ResponsibleRole { get; set; } = string.Empty; // DPO, CISO, etc.
}

public class RemediationStep
{
    public int Order { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SqlFix { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public bool RequiresApproval { get; set; }
    public string ValidationMethod { get; set; } = string.Empty;
}

/// <summary>
/// Supported compliance frameworks
/// </summary>
public enum ComplianceFramework
{
    GDPR,
    SOX,
    HIPAA,
    CCPA,
    PCI_DSS,
    ISO_27001,
    NIST,
    All
}
