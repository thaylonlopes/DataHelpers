using ClosedXML.Excel;
using DataImportExport.Helpers.Interfaces;

namespace DataImportExport.Helpers;

/// <summary>
/// Exportador de coleções de objetos tipados para pastas de trabalho Excel (.xlsx) utilizando ClosedXML.
/// </summary>
public class ExcelDataExporter : IDataExporter
{
    /// <inheritdoc/>
    public async Task ExportAsync<T>(string filePath, IEnumerable<T> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        var properties = typeof(T).GetProperties();

        for (int i = 0; i < properties.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = properties[i].Name;
        }

        var rowIndex = 2;
        foreach (var item in data)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(item);
                worksheet.Cell(rowIndex, i + 1).Value = value != null ? Convert.ToString(value) : string.Empty;
            }
            rowIndex++;
        }

        workbook.SaveAs(filePath);
        await Task.CompletedTask;
    }
}
