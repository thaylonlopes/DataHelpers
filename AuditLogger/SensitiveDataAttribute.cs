namespace AuditLogger;

/// <summary>
/// Indica que a propriedade anotada contém dados sensíveis e deve ser mascarada nas trilhas de auditoria.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SensitiveDataAttribute : Attribute
{
    /// <summary>
    /// Padrão customizado de máscara a ser exibido. Caso nulo, utiliza a máscara padrão configurada.
    /// </summary>
    public string? Mask { get; }

    /// <summary>
    /// Inicializa uma nova instância do atributo <see cref="SensitiveDataAttribute"/>.
    /// </summary>
    /// <param name="mask">Máscara customizada opcional.</param>
    public SensitiveDataAttribute(string? mask = null)
    {
        Mask = mask;
    }
}

