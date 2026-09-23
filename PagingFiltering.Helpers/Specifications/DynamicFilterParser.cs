using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using PagingFiltering.Helpers.Attributes;

namespace PagingFiltering.Helpers.Specifications;

/// <summary>
/// Operadores suportados para filtros dinâmicos.
/// </summary>
public enum FilterOperator
{
    /// <summary>Igualdade</summary>
    Equals,
    /// <summary>Diferença</summary>
    NotEquals,
    /// <summary>Maior que</summary>
    GreaterThan,
    /// <summary>Maior ou igual a</summary>
    GreaterThanOrEqual,
    /// <summary>Menor que</summary>
    LessThan,
    /// <summary>Menor ou igual a</summary>
    LessThanOrEqual,
    /// <summary>Contém (apenas texto)</summary>
    Contains,
    /// <summary>Inicia com (apenas texto)</summary>
    StartsWith
}

/// <summary>
/// Critério de filtro dinâmico.
/// </summary>
public record FilterCriterion(string PropertyName, FilterOperator Operator, object? Value);

/// <summary>
/// Parser dinâmico de critérios de filtro para composição de instâncias de <see cref="Specification{T}"/>.
/// </summary>
public static class DynamicFilterParser
{
    /// <summary>
    /// Converte um critério dinâmico em uma <see cref="Specification{T}"/>.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade a ser filtrada.</typeparam>
    /// <param name="criterion">O critério de filtragem.</param>
    /// <returns>A especificação tipada correspondente.</returns>
    public static Specification<T> Parse<T>(FilterCriterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);

        var param = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperty(criterion.PropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
            ?? throw new ArgumentException($"Propriedade '{criterion.PropertyName}' não encontrada no tipo '{typeof(T).Name}'.", nameof(criterion));

        if (property.GetCustomAttribute<FilterIgnoreAttribute>() != null)
        {
            throw new SecurityException($"A propriedade '{property.Name}' possui restrição de segurança e não pode ser utilizada como filtro.");
        }

        var left = Expression.Property(param, property);
        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        var convertedValue = criterion.Value != null
            ? Convert.ChangeType(criterion.Value, targetType)
            : null;

        var right = Expression.Constant(convertedValue, property.PropertyType);

        Expression body = criterion.Operator switch
        {
            FilterOperator.Equals => Expression.Equal(left, right),
            FilterOperator.NotEquals => Expression.NotEqual(left, right),
            FilterOperator.GreaterThan => Expression.GreaterThan(left, right),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left, right),
            FilterOperator.LessThan => Expression.LessThan(left, right),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(left, right),
            FilterOperator.Contains when property.PropertyType == typeof(string) =>
                Expression.Call(left, typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!, right),
            FilterOperator.StartsWith when property.PropertyType == typeof(string) =>
                Expression.Call(left, typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!, right),
            _ => throw new NotSupportedException($"Operador '{criterion.Operator}' não suportado para o tipo '{property.PropertyType.Name}'.")
        };

        var lambda = Expression.Lambda<Func<T, bool>>(body, param);
        return new DirectSpecification<T>(lambda);
    }
}

