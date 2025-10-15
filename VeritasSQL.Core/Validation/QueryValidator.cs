using System.Text.RegularExpressions;
using VeritasSQL.Core.Models;

namespace VeritasSQL.Core.Validation;

/// <summary>
/// Validates SQL queries against security rules
/// </summary>
public class QueryValidator
{
    private readonly SchemaInfo? _schema;
    private readonly int _defaultRowLimit;

    // Whitelist: Only SELECT allowed
    private static readonly Regex SelectOnlyRegex = new(
        @"^\s*SELECT\s+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Blacklist: Dangerous operations
    private static readonly string[] BlacklistedKeywords = new[]
    {
        "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "CREATE", "TRUNCATE",
        "EXEC", "EXECUTE", "SP_", "XP_", ";--", "/*", "*/", "DECLARE",
        "CURSOR", "BEGIN", "COMMIT", "ROLLBACK", "GRANT", "REVOKE"
    };

    // Regex for multiple statements (semicolons outside of strings)
    private static readonly Regex MultipleStatementsRegex = new(
        @";(?=(?:[^']|'[^']*')*$)",
        RegexOptions.Compiled);

    public QueryValidator(SchemaInfo? schema = null, int defaultRowLimit = 100)
    {
        _schema = schema;
        _defaultRowLimit = defaultRowLimit;
    }

    public ValidationResult Validate(string sql)
    {
        var result = new ValidationResult { IsValid = true, ModifiedSql = sql };

        if (string.IsNullOrWhiteSpace(sql))
        {
            result.IsValid = false;
            result.Issues.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Error,
                Message = "SQL query is empty"
            });
            return result;
        }

        // Check 1: Whitelist - Only SELECT
        if (!SelectOnlyRegex.IsMatch(sql))
        {
            result.IsValid = false;
            result.Issues.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Error,
                Message = "Only SELECT statements are allowed",
                Suggestion = "The query must start with SELECT"
            });
            return result;
        }

        // Check 2: Blacklist - Dangerous keywords
        foreach (var keyword in BlacklistedKeywords)
        {
            if (Regex.IsMatch(sql, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase))
            {
                result.IsValid = false;
                result.Issues.Add(new ValidationIssue
                {
                    Severity = IssueSeverity.Error,
                    Message = $"Forbidden keyword found: {keyword}",
                    Suggestion = "This keyword is not allowed for security reasons"
                });
            }
        }

        if (!result.IsValid)
            return result;

        // Check 3: Multiple Statements
        var statements = MultipleStatementsRegex.Split(sql)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        if (statements.Count > 1)
        {
            result.IsValid = false;
            result.Issues.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Error,
                Message = "Multiple statements are not allowed",
                Suggestion = "Please execute only one SELECT statement"
            });
            return result;
        }

        // Check 4: Schema Gate (if schema available)
        if (_schema != null)
        {
            var schemaIssues = ValidateAgainstSchema(sql);
            result.Issues.AddRange(schemaIssues);
            if (schemaIssues.Any(i => i.Severity == IssueSeverity.Error))
            {
                result.IsValid = false;
                return result;
            }
        }

        // Check 5: TOP/LIMIT Injection
        if (!HasRowLimit(sql))
        {
            result.ModifiedSql = InjectTopClause(sql, _defaultRowLimit);
            result.Issues.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Info,
                Message = $"TOP {_defaultRowLimit} was automatically added",
                Suggestion = "Explicitly add TOP N to limit the number of rows"
            });
        }

        // Check 6: Performance warnings
        var perfIssues = CheckPerformanceWarnings(sql);
        result.Issues.AddRange(perfIssues);

        return result;
    }

    private List<ValidationIssue> ValidateAgainstSchema(string sql)
    {
        var issues = new List<ValidationIssue>();

        // Extract table/view names from SQL (simple regex)
        var tablePattern = new Regex(
            @"(?:FROM|JOIN)\s+(\[?[\w]+\]?\.\[?[\w]+\]?|\[?[\w]+\]?)",
            RegexOptions.IgnoreCase);

        var matches = tablePattern.Matches(sql);
        foreach (Match match in matches)
        {
            var tableName = match.Groups[1].Value.Trim('[', ']');
            
            // Check if table exists in schema
            var exists = _schema!.AllObjects.Any(o => 
                o.FullName.Equals(tableName, StringComparison.OrdinalIgnoreCase) ||
                o.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                issues.Add(new ValidationIssue
                {
                    Severity = IssueSeverity.Error,
                    Message = $"Table/View '{tableName}' does not exist in schema",
                    Suggestion = "Check the table name or reload the schema"
                });
            }
        }

        return issues;
    }

    private bool HasRowLimit(string sql)
    {
        // Check for TOP N or FETCH FIRST
        return Regex.IsMatch(sql, @"\bTOP\s+\d+\b", RegexOptions.IgnoreCase) ||
               Regex.IsMatch(sql, @"\bFETCH\s+FIRST\s+\d+\s+ROWS?\s+ONLY\b", RegexOptions.IgnoreCase);
    }

    private string InjectTopClause(string sql, int limit)
    {
        // Insert TOP N after SELECT
        return Regex.Replace(
            sql,
            @"^(\s*SELECT\s+)(DISTINCT\s+)?",
            $"$1$2TOP {limit} ",
            RegexOptions.IgnoreCase);
    }

    private List<ValidationIssue> CheckPerformanceWarnings(string sql)
    {
        var issues = new List<ValidationIssue>();

        // Warning for missing WHERE
        if (!Regex.IsMatch(sql, @"\bWHERE\b", RegexOptions.IgnoreCase))
        {
            issues.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Warning,
                Message = "No WHERE clause found - might return all rows",
                Suggestion = "Consider adding a WHERE clause to filter the results"
            });
        }

        // Warning for SELECT *
        if (Regex.IsMatch(sql, @"\bSELECT\s+\*\b", RegexOptions.IgnoreCase))
        {
            issues.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Warning,
                Message = "SELECT * found - consider selecting only needed columns",
                Suggestion = "Explicit column names improve performance and clarity"
            });
        }

        return issues;
    }
}

