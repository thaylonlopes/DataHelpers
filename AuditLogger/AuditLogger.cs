using System.Reflection;
using System.Text.Json;
using AuditLogger.Interfaces;
using AuditLogger.Models;
using AuditLogger.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuditLogger;

/// <summary>
/// Utilitário para cálculo de diferenças entre objetos e mascaramento de campos sensíveis em trilhas de auditoria.
/// </summary>
public class JsonDiffCalculator
{
    private static readonly string[] HeuristicSensitiveKeywords =
    [
        "password",
        "senha",
        "token",
        "secret",
        "cpf",
        "creditcard",
        "cartao"
    ];

    private readonly AuditLoggerSettings _settings;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="JsonDiffCalculator"/> com configurações fornecidas.
    /// </summary>
    /// <param name="settings">Configurações de mascaramento e auditoria.</param>
    public JsonDiffCalculator(AuditLoggerSettings? settings = null)
    {
        _settings = settings ?? new AuditLoggerSettings();
    }

    /// <summary>
    /// Serializa um objeto em JSON aplicando as regras de mascaramento e higienização.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser serializado.</typeparam>
    /// <param name="item">Instância a ser processada.</param>
    /// <returns>Cadeia JSON com campos sensíveis mascarados.</returns>
    public string CreateMaskedPayload<T>(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var maskedMap = BuildMaskedDictionary(item);
        return JsonSerializer.Serialize(maskedMap);
    }

    /// <summary>
    /// Calcula a estrutura diferencial estruturada entre o estado original e o novo estado da entidade.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade.</typeparam>
    /// <param name="oldItem">Instância com os dados anteriores.</param>
    /// <param name="newItem">Instância com os dados atualizados.</param>
    /// <returns>JSON estruturado contendo Old, New e Diff com dados sensíveis mascarados.</returns>
    public string CreateStructuredUpdatePayload<T>(T oldItem, T newItem)
    {
        ArgumentNullException.ThrowIfNull(oldItem);
        ArgumentNullException.ThrowIfNull(newItem);

        var oldMap = BuildMaskedDictionary(oldItem);
        var newMap = BuildMaskedDictionary(newItem);
        var diffMap = BuildDifferentialMap(oldItem, newItem);

        var payload = new
        {
            Old = oldMap,
            New = newMap,
            Diff = diffMap
        };

        return JsonSerializer.Serialize(payload);
    }

    /// <summary>
    /// Determina se uma propriedade deve ser tratada como sensível por atributo explícito ou convenção heurística.
    /// </summary>
    /// <param name="property">Propriedade a inspecionar.</param>
    /// <param name="settings">Configurações ativas de auditoria.</param>
    /// <param name="effectiveMask">A máscara a ser aplicada caso sensível.</param>
    /// <returns>Verdadeiro se a propriedade for considerada sensível, falso caso contrário.</returns>
    public static bool IsPropertySensitive(PropertyInfo property, AuditLoggerSettings settings, out string effectiveMask)
    {
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(settings);

        var sensitiveAttribute = property.GetCustomAttribute<SensitiveDataAttribute>(true);
        if (sensitiveAttribute != null)
        {
            effectiveMask = sensitiveAttribute.Mask ?? settings.DefaultMask;
            return true;
        }

        if (settings.EnableHeuristicMasking && MatchesHeuristicPattern(property.Name))
        {
            effectiveMask = settings.DefaultMask;
            return true;
        }

        effectiveMask = string.Empty;
        return false;
    }

    private static bool MatchesHeuristicPattern(string propertyName)
    {
        foreach (var keyword in HeuristicSensitiveKeywords)
        {
            if (propertyName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private Dictionary<string, object?> BuildMaskedDictionary(object target)
    {
        var result = new Dictionary<string, object?>();
        var properties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        foreach (var prop in properties)
        {
            if (IsPropertySensitive(prop, _settings, out var mask))
            {
                result[prop.Name] = mask;
            }
            else
            {
                var rawValue = prop.GetValue(target);
                result[prop.Name] = SanitizeValueIfString(rawValue);
            }
        }

        return result;
    }

    private Dictionary<string, object?> BuildDifferentialMap<T>(T oldItem, T newItem)
    {
        var diffMap = new Dictionary<string, object?>();
        var targetType = oldItem?.GetType() ?? typeof(T);
        var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        foreach (var prop in properties)
        {
            var oldVal = prop.GetValue(oldItem);
            var newVal = prop.GetValue(newItem);

            if (Equals(oldVal, newVal))
            {
                continue;
            }

            if (IsPropertySensitive(prop, _settings, out var mask))
            {
                diffMap[prop.Name] = new
                {
                    From = mask,
                    To = mask
                };
            }
            else
            {
                diffMap[prop.Name] = new
                {
                    From = SanitizeValueIfString(oldVal),
                    To = SanitizeValueIfString(newVal)
                };
            }
        }

        return diffMap;
    }

    private static object? SanitizeValueIfString(object? value)
    {
        if (value is string text)
        {
            return CrlfSanitizer.Sanitize(text);
        }

        return value;
    }
}

/// <summary>
/// Registrador de auditoria com serialização estruturada em JSON,
/// suporte a mascaramento de campos sensíveis e cálculo automático de diferenças.
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly IAuditLogStorage _logStorage;
    private readonly ILogger<AuditLogger>? _logger;
    private readonly AuditLoggerSettings _settings;
    private readonly JsonDiffCalculator _diffCalculator;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="AuditLogger"/> com o storage configurado.
    /// </summary>
    /// <param name="logStorage">Provedor de armazenamento de logs.</param>
    public AuditLogger(IAuditLogStorage logStorage)
        : this(logStorage, null, null)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="AuditLogger"/> utilizando <see cref="ILogger{AuditLogger}"/>.
    /// </summary>
    /// <param name="logger">Logger para emissão estruturada de eventos.</param>
    /// <param name="options">Opções de configuração de auditoria.</param>
    public AuditLogger(ILogger<AuditLogger> logger, IOptions<AuditLoggerSettings>? options = null)
        : this(new LoggerAuditLogStorage(logger), logger, options)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="AuditLogger"/> completa com storage, logger e opções.
    /// </summary>
    /// <param name="logStorage">Provedor de persistência de logs.</param>
    /// <param name="logger">Logger opcional para emissão estruturada de eventos.</param>
    /// <param name="options">Opções de configuração.</param>
    public AuditLogger(
        IAuditLogStorage logStorage,
        ILogger<AuditLogger>? logger = null,
        IOptions<AuditLoggerSettings>? options = null)
    {
        _logStorage = logStorage ?? throw new ArgumentNullException(nameof(logStorage));
        _logger = logger;
        _settings = options?.Value ?? new AuditLoggerSettings();
        _diffCalculator = new JsonDiffCalculator(_settings);
    }

    /// <inheritdoc/>
    public Guid LogCreate<T>(T item, string userId)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var logEntry = CreateEntry("CREATE", userId, typeof(T).Name, _diffCalculator.CreateMaskedPayload(item));
        _logStorage.SaveLog(logEntry);
        EmitStructuredLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public async Task<Guid> LogCreateAsync<T>(T item, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var logEntry = CreateEntry("CREATE", userId, typeof(T).Name, _diffCalculator.CreateMaskedPayload(item));
        await _logStorage.SaveLogAsync(logEntry, cancellationToken);
        EmitStructuredLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public Guid LogRead<T>(T item, string userId)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var logEntry = CreateEntry("READ", userId, typeof(T).Name, _diffCalculator.CreateMaskedPayload(item));
        _logStorage.SaveLog(logEntry);
        EmitStructuredLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public async Task<Guid> LogReadAsync<T>(T item, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var logEntry = CreateEntry("READ", userId, typeof(T).Name, _diffCalculator.CreateMaskedPayload(item));
        await _logStorage.SaveLogAsync(logEntry, cancellationToken);
        EmitStructuredLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public Guid LogUpdate<T>(T oldItem, T newItem, string userId)
    {
        ArgumentNullException.ThrowIfNull(oldItem);
        ArgumentNullException.ThrowIfNull(newItem);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var structuredData = _diffCalculator.CreateStructuredUpdatePayload(oldItem, newItem);
        var logEntry = CreateEntry("UPDATE", userId, typeof(T).Name, structuredData);
        _logStorage.SaveLog(logEntry);
        EmitStructuredLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public async Task<Guid> LogUpdateAsync<T>(T oldItem, T newItem, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(oldItem);
        ArgumentNullException.ThrowIfNull(newItem);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var structuredData = _diffCalculator.CreateStructuredUpdatePayload(oldItem, newItem);
        var logEntry = CreateEntry("UPDATE", userId, typeof(T).Name, structuredData);
        await _logStorage.SaveLogAsync(logEntry, cancellationToken);
        EmitStructuredLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public Guid LogDelete<T>(T item, string userId)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var logEntry = CreateEntry("DELETE", userId, typeof(T).Name, _diffCalculator.CreateMaskedPayload(item));
        _logStorage.SaveLog(logEntry);
        EmitStructuredLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public async Task<Guid> LogDeleteAsync<T>(T item, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var logEntry = CreateEntry("DELETE", userId, typeof(T).Name, _diffCalculator.CreateMaskedPayload(item));
        await _logStorage.SaveLogAsync(logEntry, cancellationToken);
        EmitStructuredLog(logEntry);
        return logEntry.Id;
    }

    private static AuditLogEntry CreateEntry(string operation, string userId, string entity, string data)
    {
        return new AuditLogEntry(operation, userId, entity, data);
    }

    private void EmitStructuredLog(AuditLogEntry logEntry)
    {
        if (_logger == null)
        {
            return;
        }

        _logger.LogInformation(
            "Audit Log: {Operation} by {UserId} on {Entity} at {Timestamp} with correlation {CorrelationId} and tenant {TenantId} - Data: {Data}",
            logEntry.Operation,
            logEntry.UserId,
            logEntry.Entity,
            logEntry.Timestamp,
            logEntry.CorrelationId,
            logEntry.TenantId,
            logEntry.Data);
    }
}
