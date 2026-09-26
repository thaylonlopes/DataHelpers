using System.Text.Json;

namespace DataImportExport.Helpers.Models;

/// <summary>
/// Opções de configuração para serialização e desserialização de arquivos e streams JSON.
/// </summary>
public class JsonConfigurationOptions
{
    /// <summary>
    /// Opções do System.Text.Json configuradas para a exportação e importação.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
    {
        WriteIndented = true
    };
}
