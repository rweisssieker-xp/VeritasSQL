namespace VeritasSQL.Core.Models;

/// <summary>
/// Represents the database schema
/// </summary>
public class SchemaInfo
{
    public string DatabaseName { get; set; } = string.Empty;
    public List<TableInfo> Tables { get; set; } = new();
    public List<ViewInfo> Views { get; set; } = new();
    public DateTime LoadedAt { get; set; } = DateTime.Now;

    public IEnumerable<DatabaseObject> AllObjects => 
        Tables.Cast<DatabaseObject>().Concat(Views.Cast<DatabaseObject>());
}

public abstract class DatabaseObject
{
    public string Schema { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName => $"{Schema}.{Name}";
    public List<ColumnInfo> Columns { get; set; } = new();
}

public class TableInfo : DatabaseObject
{
    public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
}

public class ViewInfo : DatabaseObject
{
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public int? MaxLength { get; set; }
    public string? Description { get; set; }
}

public class ForeignKeyInfo
{
    public string Name { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedColumn { get; set; } = string.Empty;
}

