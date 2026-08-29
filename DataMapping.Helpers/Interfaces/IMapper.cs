using DataMapping.Helpers.Common;

namespace DataMapping.Helpers.Interfaces;

/// <summary>
/// Contrato para mapeamento direto e rápido entre objetos em memória.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Mapeia as propriedades compatíveis de um objeto de origem para um novo objeto de destino.
    /// </summary>
    /// <typeparam name="TSource">O tipo do objeto de origem.</typeparam>
    /// <typeparam name="TDestination">O tipo do objeto de destino a ser instanciado.</typeparam>
    /// <param name="source">A instância do objeto de origem (não nula).</param>
    /// <returns>A nova instância do objeto de destino com propriedades preenchidas.</returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Mapeia uma coleção inteira de objetos de origem para uma coleção de objetos de destino.
    /// </summary>
    /// <typeparam name="TSource">O tipo do item de origem.</typeparam>
    /// <typeparam name="TDestination">O tipo do item de destino.</typeparam>
    /// <param name="source">A coleção de origem.</param>
    /// <returns>A coleção mapeada de itens de destino.</returns>
    IEnumerable<TDestination> MapCollection<TSource, TDestination>(IEnumerable<TSource> source);

    /// <summary>
    /// Tenta mapear o objeto de origem retornando um <see cref="Result{T}"/> sem lançar exceções.
    /// </summary>
    /// <typeparam name="TSource">O tipo do objeto de origem.</typeparam>
    /// <typeparam name="TDestination">O tipo do objeto de destino.</typeparam>
    /// <param name="source">O objeto de origem.</param>
    /// <returns>O resultado contendo a instância mapeada ou a falha.</returns>
    Result<TDestination> TryMap<TSource, TDestination>(TSource? source);
}
