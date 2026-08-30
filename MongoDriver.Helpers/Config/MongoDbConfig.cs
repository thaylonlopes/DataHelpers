namespace MongoDriver.Helpers.Config;

/// <summary>
/// Configurações de conexão e inicialização do MongoDB.
/// </summary>
public class MongoDbConfig
{
    /// <summary>
    /// String de conexão do cluster MongoDB.
    /// </summary>
    public string ConnectionStrings { get; set; } = string.Empty;

    /// <summary>
    /// Nome do banco de dados principal.
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Flag indicando se a carga inicial de dados (seed) deve ser executada.
    /// </summary>
    public bool Seed { get; set; }
}
