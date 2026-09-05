using System.Linq.Expressions;
using System.Reflection;

namespace DataImportExport.Helpers.Models;

/// <summary>
/// Mapeamento fluente de cabeçalhos de coluna para propriedades de classes C#.
/// </summary>
/// <typeparam name="T">O tipo da entidade a ser mapeada.</typeparam>
public class ColumnMap<T>
{
    private readonly Dictionary<string, string> _propertyToColumn = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _columnToProperty = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Mapeia uma propriedade para um nome de coluna específico no arquivo.
    /// </summary>
    /// <typeparam name="TProp">O tipo da propriedade.</typeparam>
    /// <param name="propertyExpression">Expressão selecionando a propriedade.</param>
    /// <param name="columnName">O nome do cabeçalho no arquivo CSV ou Excel.</param>
    /// <returns>A própria instância do ColumnMap para encadeamento fluente.</returns>
    public ColumnMap<T> Map<TProp>(Expression<Func<T, TProp>> propertyExpression, string columnName)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        if (propertyExpression.Body is not MemberExpression memberExpr || memberExpr.Member is not PropertyInfo propInfo)
        {
            throw new ArgumentException("A expressão fornecida deve ser um acesso de propriedade direta.", nameof(propertyExpression));
        }

        var propName = propInfo.Name;
        _propertyToColumn[propName] = columnName;
        _columnToProperty[columnName] = propName;

        return this;
    }

    /// <summary>
    /// Obtém o nome da coluna associada à propriedade C#.
    /// </summary>
    public string GetColumnName(string propertyName)
    {
        return _propertyToColumn.TryGetValue(propertyName, out var col) ? col : propertyName;
    }

    /// <summary>
    /// Obtém o nome da propriedade C# associada à coluna do arquivo.
    /// </summary>
    public string GetPropertyName(string columnName)
    {
        return _columnToProperty.TryGetValue(columnName, out var prop) ? prop : columnName;
    }
}

