using System.Linq.Expressions;

namespace PagingFiltering.Helpers.Specifications;

/// <summary>
/// Especificação combinatória que avalia a conjunção lógica (AND) entre duas especificações.
/// </summary>
public class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="AndSpecification{T}"/>.
    /// </summary>
    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <inheritdoc />
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T), "x");

        var leftBody = new ParameterReplacer(leftExpression.Parameters[0], parameter).Visit(leftExpression.Body);
        var rightBody = new ParameterReplacer(rightExpression.Parameters[0], parameter).Visit(rightExpression.Body);

        var body = Expression.AndAlso(leftBody!, rightBody!);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
