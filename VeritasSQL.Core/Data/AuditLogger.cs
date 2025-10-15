using System.Data.SQLite;

namespace VeritasSQL.Core.Data;

/// <summary>
/// Logs audit events in SQLite
/// </summary>
public class AuditLogger
{
    private readonly string _dbPath;

    public AuditLogger()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VeritasSQL");
        
        Directory.CreateDirectory(appDataPath);
        _dbPath = Path.Combine(appDataPath, "audit.db");

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        connection.Open();

        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS AuditLog (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                User TEXT NOT NULL,
                Action TEXT NOT NULL,
                ConnectionProfile TEXT,
                NaturalLanguageQuery TEXT,
                GeneratedSql TEXT,
                ValidationStatus TEXT,
                ExecutionStatus TEXT,
                RowCount INTEGER,
                ExecutionTimeMs REAL,
                ErrorMessage TEXT,
                IpAddress TEXT
            )";

        using var command = new SQLiteCommand(createTableSql, connection);
        command.ExecuteNonQuery();

        // Index für Zeitstempel
        const string createIndexSql = @"
            CREATE INDEX IF NOT EXISTS idx_timestamp ON AuditLog(Timestamp DESC);";

        using var indexCommand = new SQLiteCommand(createIndexSql, connection);
        indexCommand.ExecuteNonQuery();
    }

    public async Task LogAsync(AuditEntry entry)
    {
        await using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO AuditLog (
                Timestamp, User, Action, ConnectionProfile,
                NaturalLanguageQuery, GeneratedSql, ValidationStatus,
                ExecutionStatus, RowCount, ExecutionTimeMs, ErrorMessage, IpAddress
            ) VALUES (
                @Timestamp, @User, @Action, @Profile,
                @NLQuery, @Sql, @ValidationStatus,
                @ExecStatus, @RowCount, @ExecTime, @Error, @IP
            )";

        await using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@Timestamp", entry.Timestamp.ToString("O"));
        command.Parameters.AddWithValue("@User", entry.User);
        command.Parameters.AddWithValue("@Action", entry.Action);
        command.Parameters.AddWithValue("@Profile", (object?)entry.ConnectionProfile ?? DBNull.Value);
        command.Parameters.AddWithValue("@NLQuery", (object?)entry.NaturalLanguageQuery ?? DBNull.Value);
        command.Parameters.AddWithValue("@Sql", (object?)entry.GeneratedSql ?? DBNull.Value);
        command.Parameters.AddWithValue("@ValidationStatus", (object?)entry.ValidationStatus ?? DBNull.Value);
        command.Parameters.AddWithValue("@ExecStatus", (object?)entry.ExecutionStatus ?? DBNull.Value);
        command.Parameters.AddWithValue("@RowCount", entry.RowCount.HasValue ? entry.RowCount.Value : DBNull.Value);
        command.Parameters.AddWithValue("@ExecTime", entry.ExecutionTimeMs.HasValue ? entry.ExecutionTimeMs.Value : DBNull.Value);
        command.Parameters.AddWithValue("@Error", (object?)entry.ErrorMessage ?? DBNull.Value);
        command.Parameters.AddWithValue("@IP", (object?)entry.IpAddress ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<AuditEntry>> GetLogsAsync(int limit = 100, DateTime? from = null, DateTime? to = null)
    {
        await using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await connection.OpenAsync();

        var sql = "SELECT * FROM AuditLog WHERE 1=1";
        if (from.HasValue)
            sql += " AND Timestamp >= @From";
        if (to.HasValue)
            sql += " AND Timestamp <= @To";
        sql += " ORDER BY Timestamp DESC LIMIT @Limit";

        await using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@Limit", limit);
        if (from.HasValue)
            command.Parameters.AddWithValue("@From", from.Value.ToString("O"));
        if (to.HasValue)
            command.Parameters.AddWithValue("@To", to.Value.ToString("O"));

        var entries = new List<AuditEntry>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            entries.Add(new AuditEntry
            {
                Timestamp = DateTime.Parse(reader.GetString(1)),
                User = reader.GetString(2),
                Action = reader.GetString(3),
                ConnectionProfile = reader.IsDBNull(4) ? null : reader.GetString(4),
                NaturalLanguageQuery = reader.IsDBNull(5) ? null : reader.GetString(5),
                GeneratedSql = reader.IsDBNull(6) ? null : reader.GetString(6),
                ValidationStatus = reader.IsDBNull(7) ? null : reader.GetString(7),
                ExecutionStatus = reader.IsDBNull(8) ? null : reader.GetString(8),
                RowCount = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                ExecutionTimeMs = reader.IsDBNull(10) ? null : reader.GetDouble(10),
                ErrorMessage = reader.IsDBNull(11) ? null : reader.GetString(11),
                IpAddress = reader.IsDBNull(12) ? null : reader.GetString(12)
            });
        }

        return entries;
    }
}

public class AuditEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string User { get; set; } = Environment.UserName;
    public string Action { get; set; } = string.Empty;
    public string? ConnectionProfile { get; set; }
    public string? NaturalLanguageQuery { get; set; }
    public string? GeneratedSql { get; set; }
    public string? ValidationStatus { get; set; }
    public string? ExecutionStatus { get; set; }
    public int? RowCount { get; set; }
    public double? ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? IpAddress { get; set; }
}

