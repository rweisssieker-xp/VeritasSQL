using Microsoft.Data.SqlClient;
using VeritasSQL.Core.Models;

namespace VeritasSQL.Core.Services;

/// <summary>
/// Loads database schema information
/// </summary>
public class SchemaService
{
    public async Task<SchemaInfo> LoadSchemaAsync(string connectionString)
    {
        var schema = new SchemaInfo();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        schema.DatabaseName = connection.Database;

        // Lade Tabellen
        schema.Tables = await LoadTablesAsync(connection);

        // Lade Views
        schema.Views = await LoadViewsAsync(connection);

        // Lade Spalten für alle Objekte
        await LoadColumnsAsync(connection, schema);

        // Lade Foreign Keys
        await LoadForeignKeysAsync(connection, schema);

        return schema;
    }

    private async Task<List<TableInfo>> LoadTablesAsync(SqlConnection connection)
    {
        var tables = new List<TableInfo>();

        const string query = @"
            SELECT 
                TABLE_SCHEMA,
                TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_SCHEMA, TABLE_NAME";

        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(new TableInfo
            {
                Schema = reader.GetString(0),
                Name = reader.GetString(1)
            });
        }

        return tables;
    }

    private async Task<List<ViewInfo>> LoadViewsAsync(SqlConnection connection)
    {
        var views = new List<ViewInfo>();

        const string query = @"
            SELECT 
                TABLE_SCHEMA,
                TABLE_NAME
            FROM INFORMATION_SCHEMA.VIEWS
            ORDER BY TABLE_SCHEMA, TABLE_NAME";

        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            views.Add(new ViewInfo
            {
                Schema = reader.GetString(0),
                Name = reader.GetString(1)
            });
        }

        return views;
    }

    private async Task LoadColumnsAsync(SqlConnection connection, SchemaInfo schema)
    {
        const string query = @"
            SELECT 
                c.TABLE_SCHEMA,
                c.TABLE_NAME,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE,
                c.CHARACTER_MAXIMUM_LENGTH,
                CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_PRIMARY_KEY
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN (
                SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                    ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
                    AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                    AND tc.TABLE_SCHEMA = ku.TABLE_SCHEMA
                    AND tc.TABLE_NAME = ku.TABLE_NAME
            ) pk ON c.TABLE_SCHEMA = pk.TABLE_SCHEMA 
                AND c.TABLE_NAME = pk.TABLE_NAME 
                AND c.COLUMN_NAME = pk.COLUMN_NAME
            ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION";

        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var tableSchema = reader.GetString(0);
            var tableName = reader.GetString(1);

            var dbObject = schema.AllObjects.FirstOrDefault(o => 
                o.Schema == tableSchema && o.Name == tableName);

            if (dbObject != null)
            {
                dbObject.Columns.Add(new ColumnInfo
                {
                    Name = reader.GetString(2),
                    DataType = reader.GetString(3),
                    IsNullable = reader.GetString(4) == "YES",
                    MaxLength = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    IsPrimaryKey = reader.GetInt32(6) == 1
                });
            }
        }
    }

    private async Task LoadForeignKeysAsync(SqlConnection connection, SchemaInfo schema)
    {
        const string query = @"
            SELECT 
                fk.name AS FK_NAME,
                OBJECT_SCHEMA_NAME(fk.parent_object_id) AS TABLE_SCHEMA,
                OBJECT_NAME(fk.parent_object_id) AS TABLE_NAME,
                COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS COLUMN_NAME,
                OBJECT_SCHEMA_NAME(fk.referenced_object_id) + '.' + OBJECT_NAME(fk.referenced_object_id) AS REFERENCED_TABLE,
                COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS REFERENCED_COLUMN
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc 
                ON fk.object_id = fkc.constraint_object_id
            ORDER BY TABLE_SCHEMA, TABLE_NAME";

        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var tableSchema = reader.GetString(1);
            var tableName = reader.GetString(2);

            var table = schema.Tables.FirstOrDefault(t => 
                t.Schema == tableSchema && t.Name == tableName);

            if (table != null)
            {
                table.ForeignKeys.Add(new ForeignKeyInfo
                {
                    Name = reader.GetString(0),
                    ColumnName = reader.GetString(3),
                    ReferencedTable = reader.GetString(4),
                    ReferencedColumn = reader.GetString(5)
                });
            }
        }
    }
}

