namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents a history entry
/// </summary>
public class QueryHistoryEntry
{
    public int Id { get; set; }
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string GeneratedSql { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.Now;
    public string ConnectionProfileId { get; set; } = string.Empty;
    public string ConnectionProfileName { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public double ExecutionTimeMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsFavorite { get; set; }
    public string? FavoriteName { get; set; }
    public string? FavoriteDescription { get; set; }
}

