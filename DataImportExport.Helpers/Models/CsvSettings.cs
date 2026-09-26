namespace DataImportExport.Helpers.Models;

/// <summary>
/// Opções de configuração para importação e exportação de dados em formato CSV delimitado.
/// </summary>
public class CsvSettings
{
    /// <summary>
    /// Indica se a primeira linha do arquivo contém o cabeçalho dos registros.
    /// </summary>
    public bool HasHeaderRecord { get; set; } = true;

    /// <summary>
    /// O caractere ou sequência delimitadora utilizada para separar as colunas (padrão vírgula ',').
    /// </summary>
    public string Delimiter { get; set; } = ",";
}
