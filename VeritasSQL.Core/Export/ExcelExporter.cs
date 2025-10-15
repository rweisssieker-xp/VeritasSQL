using System.Data;
using OfficeOpenXml;

namespace VeritasSQL.Core.Export;

/// <summary>
/// Exports DataTable to Excel (XLSX)
/// </summary>
public class ExcelExporter
{
    public async Task ExportAsync(
        DataTable data,
        string filePath,
        bool includeMetadata = false,
        string? sqlQuery = null,
        string? connectionProfile = null,
        int? rowCount = null)
    {
        // EPPlus 8+ automatically uses Non-Commercial License for non-commercial use
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Results");

        int currentRow = 1;

        // Metadata
        if (includeMetadata)
        {
            worksheet.Cells[currentRow, 1].Value = "VeritasSQL Export";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            currentRow++;

            worksheet.Cells[currentRow, 1].Value = "Date:";
            worksheet.Cells[currentRow, 2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            currentRow++;

            if (!string.IsNullOrEmpty(connectionProfile))
            {
                worksheet.Cells[currentRow, 1].Value = "Data Source:";
                worksheet.Cells[currentRow, 2].Value = connectionProfile;
                currentRow++;
            }

            if (rowCount.HasValue)
            {
                worksheet.Cells[currentRow, 1].Value = "Row Count:";
                worksheet.Cells[currentRow, 2].Value = rowCount.Value;
                currentRow++;
            }

            if (!string.IsNullOrEmpty(sqlQuery))
            {
                worksheet.Cells[currentRow, 1].Value = "SQL Query:";
                currentRow++;
                worksheet.Cells[currentRow, 1, currentRow, 5].Merge = true;
                worksheet.Cells[currentRow, 1].Value = sqlQuery;
                worksheet.Cells[currentRow, 1].Style.WrapText = true;
                currentRow++;
            }

            currentRow++; // Empty line
        }

        // Header
        int headerRow = currentRow;
        for (int col = 0; col < data.Columns.Count; col++)
        {
            worksheet.Cells[headerRow, col + 1].Value = data.Columns[col].ColumnName;
            worksheet.Cells[headerRow, col + 1].Style.Font.Bold = true;
            worksheet.Cells[headerRow, col + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[headerRow, col + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        currentRow++;

        // Data
        for (int row = 0; row < data.Rows.Count; row++)
        {
            for (int col = 0; col < data.Columns.Count; col++)
            {
                var value = data.Rows[row][col];
                if (value != DBNull.Value)
                {
                    // Type detection for better formatting
                    if (value is DateTime dt)
                    {
                        worksheet.Cells[currentRow, col + 1].Value = dt;
                        worksheet.Cells[currentRow, col + 1].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                    }
                    else if (value is decimal || value is double || value is float)
                    {
                        worksheet.Cells[currentRow, col + 1].Value = value;
                        worksheet.Cells[currentRow, col + 1].Style.Numberformat.Format = "#,##0.00";
                    }
                    else
                    {
                        worksheet.Cells[currentRow, col + 1].Value = value.ToString();
                    }
                }
            }
            currentRow++;
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Save
        var fileInfo = new FileInfo(filePath);
        await package.SaveAsAsync(fileInfo);
    }
}

