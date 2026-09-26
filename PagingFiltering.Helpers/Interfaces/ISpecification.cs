using System.Linq.Expressions;

namespace PagingFiltering.Helpers.Interfaces;

/// <summary>
/// Contrato base do Specification Pattern para encapsulamento de regras de predicado combináveis.
/// </summary>
/// <typeparam name="T">O tipo da entidade a ser validada pela especificação.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Converte a especificação em uma expressão lambda de predicado Linq.
    /// </summary>
    /// <returns>Expressão booleana para consulta.</returns>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Avalia se uma instância específica satisfaz a regra da especificação.
    /// </summary>
    /// <param name="entity">A entidade a testar.</param>
    /// <returns><c>true</c> se a especificação for satisfeita; caso contrário, <c>false</c>.</returns>
    bool IsSatisfiedBy(T entity);
}
