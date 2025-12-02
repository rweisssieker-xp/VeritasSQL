namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents a reusable query template
/// </summary>
public class QueryTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Analytics, Reporting, DataQuality, Admin
    public string NaturalLanguageTemplate { get; set; } = string.Empty;
    public string SqlTemplate { get; set; } = string.Empty;
    public List<TemplateParameter> Parameters { get; set; } = new();
    public bool IsBuiltIn { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a parameter in a query template
/// </summary>
public class TemplateParameter
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public string Type { get; set; } = "string"; // string, number, date, table, column
}

/// <summary>
/// Built-in query templates
/// </summary>
public static class BuiltInTemplates
{
    public static List<QueryTemplate> GetAll() => new()
    {
        // Analytics Templates
        new QueryTemplate
        {
            Name = "Top N Records",
            Description = "Get the top N records from a table ordered by a column",
            Category = "Analytics",
            NaturalLanguageTemplate = "Show me the top {count} {table} ordered by {column} descending",
            SqlTemplate = "SELECT TOP {count} * FROM {table} ORDER BY {column} DESC",
            Parameters = new()
            {
                new TemplateParameter { Name = "count", Description = "Number of records", DefaultValue = "10", Type = "number" },
                new TemplateParameter { Name = "table", Description = "Table name", Type = "table" },
                new TemplateParameter { Name = "column", Description = "Column to order by", Type = "column" }
            },
            IsBuiltIn = true
        },
        new QueryTemplate
        {
            Name = "Count by Category",
            Description = "Count records grouped by a category column",
            Category = "Analytics",
            NaturalLanguageTemplate = "How many {table} are there per {groupColumn}?",
            SqlTemplate = "SELECT {groupColumn}, COUNT(*) AS Count FROM {table} GROUP BY {groupColumn} ORDER BY Count DESC",
            Parameters = new()
            {
                new TemplateParameter { Name = "table", Description = "Table name", Type = "table" },
                new TemplateParameter { Name = "groupColumn", Description = "Column to group by", Type = "column" }
            },
            IsBuiltIn = true
        },
        new QueryTemplate
        {
            Name = "Sum by Category",
            Description = "Sum a numeric column grouped by category",
            Category = "Analytics",
            NaturalLanguageTemplate = "What is the total {sumColumn} per {groupColumn}?",
            SqlTemplate = "SELECT {groupColumn}, SUM({sumColumn}) AS Total FROM {table} GROUP BY {groupColumn} ORDER BY Total DESC",
            Parameters = new()
            {
                new TemplateParameter { Name = "table", Description = "Table name", Type = "table" },
                new TemplateParameter { Name = "sumColumn", Description = "Column to sum", Type = "column" },
                new TemplateParameter { Name = "groupColumn", Description = "Column to group by", Type = "column" }
            },
            IsBuiltIn = true
        },
        new QueryTemplate
        {
            Name = "Average by Category",
            Description = "Calculate average of a numeric column grouped by category",
            Category = "Analytics",
            NaturalLanguageTemplate = "What is the average {avgColumn} per {groupColumn}?",
            SqlTemplate = "SELECT {groupColumn}, AVG({avgColumn}) AS Average FROM {table} GROUP BY {groupColumn} ORDER BY Average DESC",
            Parameters = new()
            {
                new TemplateParameter { Name = "table", Description = "Table name", Type = "table" },
                new TemplateParameter { Name = "avgColumn", Description = "Column to average", Type = "column" },
                new TemplateParameter { Name = "groupColumn", Description = "Column to group by", Type = "column" }
            },
            IsBuiltIn = true
        },

        // Reporting Templates
        new QueryTemplate
        {
            Name = "Date Range Filter",
            Description = "Filter records within a date range",
            Category = "Reporting",
            NaturalLanguageTemplate = "Show {table} where {dateColumn} is between {startDate} and {endDate}",
            SqlTemplate = "SELECT * FROM {table} WHERE {dateColumn} BETWEEN '{startDate}' AND '{endDate}'",
            Parameters = new()
            {
                new TemplateParameter { Name = "table", Description = "Table name", Type = "table" },
                new TemplateParameter { Name = "dateColumn", Description = "Date column", Type = "column" },
                new TemplateParameter { Name = "startDate", Description = "Start date", DefaultValue = "2024-01-01", Type = "date" },
                new TemplateParameter { Name = "endDate", Description = "End date", DefaultValue = "2024-12-31", Type = "date" }
            },
            IsBuiltIn = true
        },
        new QueryTemplate
        {
            Name = "Monthly Trend",
            Description = "Show monthly trend of a metric",
            Category = "Reporting",
            NaturalLanguageTemplate = "Show monthly {aggregation} of {column} from {table}",
            SqlTemplate = "SELECT YEAR({dateColumn}) AS Year, MONTH({dateColumn}) AS Month, {aggregation}({column}) AS Value FROM {table} GROUP BY YEAR({dateColumn}), MONTH({dateColumn}) ORDER BY Year, Month",
            Parameters = new()
            {
                new TemplateParameter { Name = "table", Description = "Table name", Type = "table" },
                new TemplateParameter { Name = "dateColumn", Description = "Date column", Type = "column" },
                new TemplateParameter { Name = "column", Description = "Column to aggregate", Type = "column" },
                new TemplateParameter { Name = "aggregation", Description = "Aggregation function", DefaultValue = "SUM", Type = "string" }
            },
            IsBuiltIn = true
        },

        // Data Quality Templates
        new QueryTemplate
        {
            Name = "Find Null Values",
            Description = "Find records with null values in a specific column",
            Category = "DataQuality",
            NaturalLanguageTemplate = "Show {table} where {column} is null",
            SqlTemplate = "SELECT * FROM {table} WHERE {column} IS NULL",
            Parameters = new()
            {
                new TemplateParameter { Name = "table", Description = "Table name", Type = "table" },
                new TemplateParameter { Name = "column", Description = "Column to check", Type = "column" }
            },
            IsBuiltIn = true
        },
        new QueryTemplate
        {
            Name = "Find Duplicates",
            Description = "Find duplicate values in a column",
            Category = "DataQuality",
            NaturalLanguageTemplate = "Find duplicate {column} values in {table}",
            SqlTemplate = "SELECT {column}, COUNT(*) AS DuplicateCount FROM {table} GROUP BY {column} HAVING COUNT(*) > 1 ORDER BY DuplicateCount DESC",
            Parameters = new()
            {
                new TemplateParameter { Name = "table", Description = "Table name", Type = "table" },
                new TemplateParameter { Name = "column", Description = "Column to check for duplicates", Type = "column" }
            },
            IsBuiltIn = true
        },
        new QueryTemplate
        {
            Name = "Column Statistics",
            Description = "Get basic statistics for a numeric column",
            Category = "DataQuality",
            NaturalLanguageTemplate = "Show statistics for {column} in {table}",
            SqlTemplate = "SELECT COUNT(*) AS TotalRows, COUNT({column}) AS NonNullCount, AVG({column}) AS Average, MIN({column}) AS Minimum, MAX({column}) AS Maximum, STDEV({column}) AS StdDev FROM {table}",
            Parameters = new()
            {
                new TemplateParameter { Name = "table", Description = "Table name", Type = "table" },
                new TemplateParameter { Name = "column", Description = "Numeric column", Type = "column" }
            },
            IsBuiltIn = true
        },

        // Join Templates
        new QueryTemplate
        {
            Name = "Inner Join",
            Description = "Join two tables on a common column",
            Category = "Joins",
            NaturalLanguageTemplate = "Join {table1} with {table2} on {joinColumn}",
            SqlTemplate = "SELECT * FROM {table1} t1 INNER JOIN {table2} t2 ON t1.{joinColumn} = t2.{joinColumn}",
            Parameters = new()
            {
                new TemplateParameter { Name = "table1", Description = "First table", Type = "table" },
                new TemplateParameter { Name = "table2", Description = "Second table", Type = "table" },
                new TemplateParameter { Name = "joinColumn", Description = "Column to join on", Type = "column" }
            },
            IsBuiltIn = true
        },
        new QueryTemplate
        {
            Name = "Left Join with Count",
            Description = "Left join and count related records",
            Category = "Joins",
            NaturalLanguageTemplate = "Show {table1} with count of related {table2}",
            SqlTemplate = "SELECT t1.*, COUNT(t2.{countColumn}) AS RelatedCount FROM {table1} t1 LEFT JOIN {table2} t2 ON t1.{joinColumn} = t2.{joinColumn} GROUP BY t1.{groupColumns}",
            Parameters = new()
            {
                new TemplateParameter { Name = "table1", Description = "Main table", Type = "table" },
                new TemplateParameter { Name = "table2", Description = "Related table", Type = "table" },
                new TemplateParameter { Name = "joinColumn", Description = "Column to join on", Type = "column" },
                new TemplateParameter { Name = "countColumn", Description = "Column to count", Type = "column" },
                new TemplateParameter { Name = "groupColumns", Description = "Columns to group by", Type = "string" }
            },
            IsBuiltIn = true
        }
    };
}

