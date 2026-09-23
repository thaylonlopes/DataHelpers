using ClosedXML.Excel;
using DataImportExport.Helpers.Interfaces;

namespace DataImportExport.Helpers;

/// <summary>
/// Importador de dados em formato Excel (XLSX) utilizando ClosedXML com guardrail de memória contra OOM.
/// </summary>
public class ExcelDataImporter : IDataImporter
{
    /// <summary>
    /// Limite padrão de segurança de linhas para importação Excel (15.000 linhas).
    /// </summary>
    public const int DefaultMaxRowsLimit = 15_000;

    /// <summary>
    /// Limite máximo configurado de linhas de dados suportadas antes de disparar exceção preventiva de OOM.
    /// </summary>
    public int MaxRowsLimit { get; }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="ExcelDataImporter"/>.
    /// </summary>
    /// <param name="maxRowsLimit">Limite máximo de linhas permitido (padrão 15.000).</param>
    public ExcelDataImporter(int maxRowsLimit = DefaultMaxRowsLimit)
    {
        if (maxRowsLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRowsLimit), "O limite máximo de linhas deve ser maior que zero.");
        }
        MaxRowsLimit = maxRowsLimit;
    }

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

        var dataRowCount = rows.Count - 1;
        if (dataRowCount > MaxRowsLimit)
        {
            throw new InvalidOperationException(
                $"O arquivo Excel ultrapassa o limite de segurança de {MaxRowsLimit} linhas ({dataRowCount} linhas encontradas). " +
                "Para volumes massivos, utilize o importador CSV via streaming (SpanDelimitedParser) para prevenir esgotamento de memória (OOM).");
        }

        var headerRow = rows[0];
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.Cells())
        {
            headers[cell.GetString()] = cell.Address.ColumnNumber;
        }

        var records = new List<T>(dataRowCount);
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
