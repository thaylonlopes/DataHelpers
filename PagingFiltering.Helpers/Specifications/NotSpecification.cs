using System.Linq.Expressions;

namespace PagingFiltering.Helpers.Specifications;

/// <summary>
/// Especificação combinatória que avalia a negação lógica (NOT) de uma especificação.
/// </summary>
public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="NotSpecification{T}"/>.
    /// </summary>
    public NotSpecification(Specification<T> specification)
    {
        _specification = specification ?? throw new ArgumentNullException(nameof(specification));
    }

    /// <inheritdoc />
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");

        var body = new ParameterReplacer(expression.Parameters[0], parameter).Visit(expression.Body);
        var negated = Expression.Not(body!);

        return Expression.Lambda<Func<T, bool>>(negated, parameter);
    }
}
