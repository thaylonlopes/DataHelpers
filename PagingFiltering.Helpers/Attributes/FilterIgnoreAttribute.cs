namespace PagingFiltering.Helpers.Attributes;

/// <summary>
/// Indica que a propriedade não pode ser utilizada como critério de filtragem dinâmica por motivos de segurança.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FilterIgnoreAttribute : Attribute
{
}
