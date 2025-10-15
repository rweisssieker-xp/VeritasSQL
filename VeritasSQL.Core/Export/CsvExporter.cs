using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace VeritasSQL.Core.Export;

/// <summary>
/// Exports DataTable to CSV
/// </summary>
public class CsvExporter
{
    public async Task ExportAsync(
        DataTable data, 
        string filePath, 
        bool includeMetadata = false,
        string? sqlQuery = null,
        string? connectionProfile = null,
        int? rowCount = null)
    {
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("de-DE"))
        {
            Delimiter = ";",
            HasHeaderRecord = true
        };

        await using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        await using var csv = new CsvWriter(writer, config);

        // Metadata as comments
        if (includeMetadata)
        {
            await writer.WriteLineAsync($"# VeritasSQL Export");
            await writer.WriteLineAsync($"# Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            if (!string.IsNullOrEmpty(connectionProfile))
                await writer.WriteLineAsync($"# Data Source: {connectionProfile}");
            if (rowCount.HasValue)
                await writer.WriteLineAsync($"# Row Count: {rowCount.Value}");
            if (!string.IsNullOrEmpty(sqlQuery))
            {
                await writer.WriteLineAsync($"# SQL Query:");
                foreach (var line in sqlQuery.Split('\n'))
                    await writer.WriteLineAsync($"# {line.Trim()}");
            }
            await writer.WriteLineAsync("#");
        }

        // Header
        foreach (DataColumn column in data.Columns)
        {
            csv.WriteField(column.ColumnName);
        }
        await csv.NextRecordAsync();

        // Data
        foreach (DataRow row in data.Rows)
        {
            foreach (DataColumn column in data.Columns)
            {
                var value = row[column];
                if (value == DBNull.Value)
                {
                    csv.WriteField(string.Empty);
                }
                else
                {
                    csv.WriteField(value);
                }
            }
            await csv.NextRecordAsync();
        }
    }
}

