using ClosedXML.Excel;
using DataImportExport.Helpers.Interfaces;

namespace DataImportExport.Helpers;

/// <summary>
/// Importador de dados em formato Excel (XLSX) utilizando ClosedXML.
/// </summary>
public class ExcelDataImporter : IDataImporter
{
    /// <inheritdoc/>
    public Task<IEnumerable<T>> ImportAsync<T>(string filePath) where T : new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Excel file not found at {filePath}", filePath);

        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            return Task.FromResult(Enumerable.Empty<T>());
        }

        var rows = worksheet.RangeUsed()?.RowsUsed()?.ToList();
        if (rows == null || rows.Count <= 1)
        {
            return Task.FromResult(Enumerable.Empty<T>());
        }

        var headerRow = rows[0];
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.Cells())
        {
            headers[cell.GetString()] = cell.Address.ColumnNumber;
        }

        var records = new List<T>();
        var properties = typeof(T).GetProperties();

        foreach (var row in rows.Skip(1))
        {
            var record = new T();
            foreach (var property in properties)
            {
                if (headers.TryGetValue(property.Name, out var colNum))
                {
                    var cell = row.Cell(colNum);
                    var cellValue = cell.GetValue<string>() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        var convertedValue = Convert.ChangeType(cellValue, targetType);
                        property.SetValue(record, convertedValue);
                    }
                }
            }
            records.Add(record);
        }

        return Task.FromResult((IEnumerable<T>)records);
    }
}
