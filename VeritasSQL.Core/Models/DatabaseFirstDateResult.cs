namespace VeritasSQL.Core.Models;

/// <summary>
/// Result of the "Database First Date" feature - AI-generated queries to explore a new database
/// Inspired by Tinder's icebreakers: "5 questions to know your new database!"
/// </summary>
public class DatabaseFirstDateResult
{
    /// <summary>
    /// A fun nickname for the database based on its content
    /// </summary>
    public string DatabaseNickname { get; set; } = string.Empty;
    
    /// <summary>
    /// A personality description of the database
    /// </summary>
    public string DatabasePersonality { get; set; } = string.Empty;
    
    /// <summary>
    /// The 5 "first date" queries to explore the database
    /// </summary>
    public List<FirstDateQuery> Queries { get; set; } = new();
    
    /// <summary>
    /// Summary of what the database contains
    /// </summary>
    public string Summary { get; set; } = string.Empty;
    
    /// <summary>
    /// Recommendations for exploring the database further
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// A single "first date" query for exploring a database
/// </summary>
public class FirstDateQuery
{
    /// <summary>
    /// Order in the sequence (1-5)
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Catchy title for the query
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// What this query reveals
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// The actual SQL query
    /// </summary>
    public string Sql { get; set; } = string.Empty;
    
    /// <summary>
    /// Why this query is valuable
    /// </summary>
    public string Insight { get; set; } = string.Empty;
    
    /// <summary>
    /// Emoji for visual appeal
    /// </summary>
    public string Emoji { get; set; } = "🔍";
    
    /// <summary>
    /// Category of insight (Core Data, Freshness, Relationships, Quality, Insight)
    /// </summary>
    public string Category { get; set; } = string.Empty;
}

