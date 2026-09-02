using AuditLogger.Interfaces;
using AuditLogger.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AuditLogger.Services;

/// <summary>
/// Provedor de armazenamento em memória utilizando <see cref="IMemoryCache"/>.
/// </summary>
public class MemoryCacheAuditLogStorage : IAuditLogStorage
{
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _cacheDuration;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="MemoryCacheAuditLogStorage"/>.
    /// </summary>
    /// <param name="memoryCache">Instância de cache em memória.</param>
    /// <param name="cacheDuration">Duração de retenção do log no cache.</param>
    public MemoryCacheAuditLogStorage(IMemoryCache memoryCache, TimeSpan cacheDuration)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _cacheDuration = cacheDuration;
    }

    /// <inheritdoc/>
    public void SaveLog(AuditLogEntry logEntry)
    {
        ArgumentNullException.ThrowIfNull(logEntry);
        var cacheKey = $"AuditLog_{logEntry.Id}";
        _memoryCache.Set(cacheKey, logEntry, _cacheDuration);
    }

    /// <summary>
    /// Recupera uma entrada de auditoria pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador do log.</param>
    /// <returns>Entrada encontrada ou null.</returns>
    public AuditLogEntry? GetLog(Guid id)
    {
        _memoryCache.TryGetValue($"AuditLog_{id}", out AuditLogEntry? logEntry);
        return logEntry;
    }
}
