namespace DataImportExport.Helpers.Models;

/// <summary>
/// Configurações específicas para importação e exportação de planilhas Excel.
/// </summary>
public class ExcelSettings
{
    /// <summary>
    /// Indica se um workbook existente deve ser utilizado em vez de criar um novo.
    /// </summary>
    public bool UseExistingWorkbook { get; set; }

    /// <summary>
    /// O nome da planilha (worksheet) a ser processada ou gerada.
    /// </summary>
    public string SheetName { get; set; } = "Sheet1";
}
