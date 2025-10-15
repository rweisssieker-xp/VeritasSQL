using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using VeritasSQL.Core.Models;

namespace VeritasSQL.Core.Services;

/// <summary>
/// Executes SQL queries
/// </summary>
public class QueryExecutor
{
    private readonly int _commandTimeout;

    public QueryExecutor(int commandTimeout = 30)
    {
        _commandTimeout = commandTimeout;
    }

    public async Task<QueryResult> ExecuteQueryAsync(string connectionString, string sql)
    {
        var result = new QueryResult { SqlQuery = sql };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = _commandTimeout,
                CommandType = CommandType.Text
            };

            await using var reader = await command.ExecuteReaderAsync();
            
            var dataTable = new DataTable();
            dataTable.Load(reader);

            stopwatch.Stop();

            result.Success = true;
            result.Data = dataTable;
            result.RowCount = dataTable.Rows.Count;
            result.ExecutionTime = stopwatch.Elapsed;
        }
        catch (SqlException ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = FormatSqlError(ex);
            result.ExecutionTime = stopwatch.Elapsed;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = $"Error during execution: {ex.Message}";
            result.ExecutionTime = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<int> EstimateRowCountAsync(string connectionString, string sql)
    {
        try
        {
            // Extract FROM clause and estimate with COUNT(*)
            var countSql = ConvertToCountQuery(sql);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(countSql, connection)
            {
                CommandTimeout = 5 // Short timeout for estimation
            };

            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : -1;
        }
        catch
        {
            return -1; // Estimation not possible
        }
    }

    private string ConvertToCountQuery(string sql)
    {
        // Simple conversion: Replace SELECT ... with SELECT COUNT(*)
        // Remove ORDER BY (not needed for COUNT)
        var countSql = System.Text.RegularExpressions.Regex.Replace(
            sql,
            @"SELECT\s+TOP\s+\d+\s+.*?\s+FROM",
            "SELECT COUNT(*) FROM",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
            System.Text.RegularExpressions.RegexOptions.Singleline);

        countSql = System.Text.RegularExpressions.Regex.Replace(
            countSql,
            @"SELECT\s+.*?\s+FROM",
            "SELECT COUNT(*) FROM",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
            System.Text.RegularExpressions.RegexOptions.Singleline);

        // Remove ORDER BY
        countSql = System.Text.RegularExpressions.Regex.Replace(
            countSql,
            @"\s+ORDER\s+BY\s+.*$",
            "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return countSql;
    }

    private string FormatSqlError(SqlException ex)
    {
        return $"SQL Error (Number {ex.Number}): {ex.Message}";
    }
}

