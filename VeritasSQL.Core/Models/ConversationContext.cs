namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents conversational chat context for multi-turn interactions
/// </summary>
public class ConversationContext
{
    public string ConversationId { get; set; } = Guid.NewGuid().ToString();
    public List<ConversationTurn> Turns { get; set; } = new();
    public Dictionary<string, string> Variables { get; set; } = new(); // user-defined variables
    public List<string> ReferencedTables { get; set; } = new();
    public string CurrentIntent { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Individual turn in conversation
/// </summary>
public class ConversationTurn
{
    public string UserMessage { get; set; } = string.Empty;
    public string AssistantResponse { get; set; } = string.Empty;
    public string? GeneratedSql { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Chat response with context awareness
/// </summary>
public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Sql { get; set; }
    public List<string> Suggestions { get; set; } = new();
    public bool NeedsMoreInfo { get; set; }
    public List<string> ClarifyingQuestions { get; set; } = new();
}
