using System.Text.Json;

namespace DataImportExport.Helpers.Models;

/// <summary>
/// Configurações consolidadas para os motores de importação e exportação (CSV, Excel e JSON).
/// </summary>
public class ImportExportSettings
{
    /// <summary>
    /// Configurações específicas para operações delimitadas (CSV/TSV).
    /// </summary>
    public required CsvSettings CsvSettings { get; set; } = null!;

    /// <summary>
    /// Opções de serialização do System.Text.Json para importação e exportação JSON.
    /// </summary>
    public required JsonSerializerOptions JsonSerializerOptions { get; set; } = null!;

    /// <summary>
    /// Configurações para planilhas eletrônicas Excel.
    /// </summary>
    public required ExcelSettings ExcelSettings { get; set; } = null!;
}
