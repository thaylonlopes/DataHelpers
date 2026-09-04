using PagingFiltering.Helpers.Interfaces;
using System.Linq.Expressions;

namespace PagingFiltering.Helpers.Specifications;

/// <summary>
/// Classe base abstrata para o Specification Pattern combinatório com suporte a operadores booleanos e encadeamento fluente.
/// </summary>
/// <typeparam name="T">O tipo da entidade.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    /// <summary>
    /// Retorna a expressão de árvore representando o predicado da especificação.
    /// </summary>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Avalia se a entidade satisfaz a especificação em memória.
    /// </summary>
    /// <param name="entity">A entidade a ser testada.</param>
    /// <returns>Verdadeiro se a entidade satisfaz o predicado.</returns>
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>
    /// Combina esta especificação com outra utilizando o operador lógico AND.
    /// </summary>
    public Specification<T> And(Specification<T> other) => new AndSpecification<T>(this, other);

    /// <summary>
    /// Combina esta especificação com outra utilizando o operador lógico OR.
    /// </summary>
    public Specification<T> Or(Specification<T> other) => new OrSpecification<T>(this, other);

    /// <summary>
    /// Nega a especificação atual utilizando o operador lógico NOT.
    /// </summary>
    public Specification<T> Not() => new NotSpecification<T>(this);

    /// <summary>
    /// Operador lógico &amp; para combinação AND.
    /// </summary>
    public static Specification<T> operator &(Specification<T> left, Specification<T> right) => new AndSpecification<T>(left, right);

    /// <summary>
    /// Operador lógico | para combinação OR.
    /// </summary>
    public static Specification<T> operator |(Specification<T> left, Specification<T> right) => new OrSpecification<T>(left, right);

    /// <summary>
    /// Operador lógico ! para negação NOT.
    /// </summary>
    public static Specification<T> operator !(Specification<T> specification) => new NotSpecification<T>(specification);
}
