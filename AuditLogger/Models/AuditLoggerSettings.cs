namespace AuditLogger.Models;

/// <summary>
/// Configurações de tempo de retenção, políticas de mascaramento de dados sensíveis e provedor de armazenamento do AuditLogger.
/// </summary>
public class AuditLoggerSettings
{
    /// <summary>
    /// Tempo de expiração do cache em memória quando utilizado MemoryCacheAuditLogStorage.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Tipo de armazenamento a ser utilizado (ex: "MemoryCache", "Logger", "ILogger", "Console").
    /// </summary>
    public string StorageType { get; set; } = "MemoryCache";

    /// <summary>
    /// Habilita o mascaramento heurístico automático baseado em convenção de nomes (ex: Password, Token, Secret, Cpf, CreditCard). Padrão é true.
    /// </summary>
    public bool EnableHeuristicMasking { get; set; } = true;

    /// <summary>
    /// Padrão padrão de máscara aplicado a dados sensíveis. Padrão é "***".
    /// </summary>
    public string DefaultMask { get; set; } = "***";
}

/// <summary>
/// Alias de configuração de opções de auditoria para compatibilidade.
/// </summary>
public class AuditLoggerOptions : AuditLoggerSettings
{
}
