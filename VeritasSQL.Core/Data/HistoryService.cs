using System.Data;
using System.Data.SQLite;
using VeritasSQL.Core.Models;

namespace VeritasSQL.Core.Data;

/// <summary>
/// Manages query history and favorites in SQLite
/// </summary>
public class HistoryService
{
    private readonly string _dbPath;

    public HistoryService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VeritasSQL");
        
        Directory.CreateDirectory(appDataPath);
        _dbPath = Path.Combine(appDataPath, "history.db");

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        connection.Open();

        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS QueryHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NaturalLanguageQuery TEXT NOT NULL,
                GeneratedSql TEXT NOT NULL,
                ExecutedAt TEXT NOT NULL,
                ConnectionProfileId TEXT NOT NULL,
                ConnectionProfileName TEXT NOT NULL,
                RowCount INTEGER NOT NULL,
                ExecutionTimeMs REAL NOT NULL,
                Success INTEGER NOT NULL,
                ErrorMessage TEXT,
                IsFavorite INTEGER NOT NULL DEFAULT 0,
                FavoriteName TEXT,
                FavoriteDescription TEXT
            )";

        using var command = new SQLiteCommand(createTableSql, connection);
        command.ExecuteNonQuery();

        // Index für schnelle Suche
        const string createIndexSql = @"
            CREATE INDEX IF NOT EXISTS idx_executed_at ON QueryHistory(ExecutedAt DESC);
            CREATE INDEX IF NOT EXISTS idx_is_favorite ON QueryHistory(IsFavorite);";

        using var indexCommand = new SQLiteCommand(createIndexSql, connection);
        indexCommand.ExecuteNonQuery();
    }

    public async Task<int> AddEntryAsync(QueryHistoryEntry entry)
    {
        await using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO QueryHistory (
                NaturalLanguageQuery, GeneratedSql, ExecutedAt, 
                ConnectionProfileId, ConnectionProfileName,
                RowCount, ExecutionTimeMs, Success, ErrorMessage,
                IsFavorite, FavoriteName, FavoriteDescription
            ) VALUES (
                @NLQuery, @Sql, @ExecutedAt,
                @ProfileId, @ProfileName,
                @RowCount, @ExecTime, @Success, @Error,
                @IsFav, @FavName, @FavDesc
            );
            SELECT last_insert_rowid();";

        await using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@NLQuery", entry.NaturalLanguageQuery);
        command.Parameters.AddWithValue("@Sql", entry.GeneratedSql);
        command.Parameters.AddWithValue("@ExecutedAt", entry.ExecutedAt.ToString("O"));
        command.Parameters.AddWithValue("@ProfileId", entry.ConnectionProfileId);
        command.Parameters.AddWithValue("@ProfileName", entry.ConnectionProfileName);
        command.Parameters.AddWithValue("@RowCount", entry.RowCount);
        command.Parameters.AddWithValue("@ExecTime", entry.ExecutionTimeMs);
        command.Parameters.AddWithValue("@Success", entry.Success ? 1 : 0);
        command.Parameters.AddWithValue("@Error", (object?)entry.ErrorMessage ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsFav", entry.IsFavorite ? 1 : 0);
        command.Parameters.AddWithValue("@FavName", (object?)entry.FavoriteName ?? DBNull.Value);
        command.Parameters.AddWithValue("@FavDesc", (object?)entry.FavoriteDescription ?? DBNull.Value);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<QueryHistoryEntry>> GetHistoryAsync(int limit = 100, int offset = 0)
    {
        await using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await connection.OpenAsync();

        const string sql = @"
            SELECT * FROM QueryHistory 
            ORDER BY ExecutedAt DESC 
            LIMIT @Limit OFFSET @Offset";

        await using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@Limit", limit);
        command.Parameters.AddWithValue("@Offset", offset);

        return await ReadEntriesAsync(command);
    }

    public async Task<List<QueryHistoryEntry>> SearchHistoryAsync(string searchTerm)
    {
        await using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await connection.OpenAsync();

        const string sql = @"
            SELECT * FROM QueryHistory 
            WHERE NaturalLanguageQuery LIKE @Search 
               OR GeneratedSql LIKE @Search
               OR ConnectionProfileName LIKE @Search
            ORDER BY ExecutedAt DESC 
            LIMIT 100";

        await using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@Search", $"%{searchTerm}%");

        return await ReadEntriesAsync(command);
    }

    public async Task<List<QueryHistoryEntry>> GetFavoritesAsync()
    {
        await using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await connection.OpenAsync();

        const string sql = @"
            SELECT * FROM QueryHistory 
            WHERE IsFavorite = 1
            ORDER BY FavoriteName";

        await using var command = new SQLiteCommand(sql, connection);
        return await ReadEntriesAsync(command);
    }

    public async Task UpdateFavoriteAsync(int id, bool isFavorite, string? name = null, string? description = null)
    {
        await using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await connection.OpenAsync();

        const string sql = @"
            UPDATE QueryHistory 
            SET IsFavorite = @IsFav,
                FavoriteName = @Name,
                FavoriteDescription = @Desc
            WHERE Id = @Id";

        await using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@IsFav", isFavorite ? 1 : 0);
        command.Parameters.AddWithValue("@Name", (object?)name ?? DBNull.Value);
        command.Parameters.AddWithValue("@Desc", (object?)description ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Updates a complete history entry
    /// </summary>
    public async Task UpdateEntryAsync(QueryHistoryEntry entry)
    {
        await using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await connection.OpenAsync();

        const string sql = @"
            UPDATE QueryHistory 
            SET NaturalLanguageQuery = @NLQuery,
                GeneratedSql = @Sql,
                IsFavorite = @IsFav,
                FavoriteName = @FavName,
                FavoriteDescription = @FavDesc
            WHERE Id = @Id";

        await using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", entry.Id);
        command.Parameters.AddWithValue("@NLQuery", entry.NaturalLanguageQuery);
        command.Parameters.AddWithValue("@Sql", entry.GeneratedSql);
        command.Parameters.AddWithValue("@IsFav", entry.IsFavorite ? 1 : 0);
        command.Parameters.AddWithValue("@FavName", (object?)entry.FavoriteName ?? DBNull.Value);
        command.Parameters.AddWithValue("@FavDesc", (object?)entry.FavoriteDescription ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteEntryAsync(int id)
    {
        await using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await connection.OpenAsync();

        const string sql = "DELETE FROM QueryHistory WHERE Id = @Id";

        await using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync();
    }

    private async Task<List<QueryHistoryEntry>> ReadEntriesAsync(SQLiteCommand command)
    {
        var entries = new List<QueryHistoryEntry>();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            entries.Add(new QueryHistoryEntry
            {
                Id = reader.GetInt32(0),
                NaturalLanguageQuery = reader.GetString(1),
                GeneratedSql = reader.GetString(2),
                ExecutedAt = DateTime.Parse(reader.GetString(3)),
                ConnectionProfileId = reader.GetString(4),
                ConnectionProfileName = reader.GetString(5),
                RowCount = reader.GetInt32(6),
                ExecutionTimeMs = reader.GetDouble(7),
                Success = reader.GetInt32(8) == 1,
                ErrorMessage = reader.IsDBNull(9) ? null : reader.GetString(9),
                IsFavorite = reader.GetInt32(10) == 1,
                FavoriteName = reader.IsDBNull(11) ? null : reader.GetString(11),
                FavoriteDescription = reader.IsDBNull(12) ? null : reader.GetString(12)
            });
        }

        return entries;
    }
}

