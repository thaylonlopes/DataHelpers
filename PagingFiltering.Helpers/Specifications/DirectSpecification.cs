using System.Linq.Expressions;

namespace PagingFiltering.Helpers.Specifications;

/// <summary>
/// Especificação direta baseada em uma expressão lambda.
/// </summary>
/// <typeparam name="T">O tipo da entidade.</typeparam>
public class DirectSpecification<T> : Specification<T>
{
    private readonly Expression<Func<T, bool>> _expression;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DirectSpecification{T}"/>.
    /// </summary>
    /// <param name="expression">A expressão de predicado.</param>
    public DirectSpecification(Expression<Func<T, bool>> expression)
    {
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    /// <inheritdoc />
    public override Expression<Func<T, bool>> ToExpression() => _expression;
}

