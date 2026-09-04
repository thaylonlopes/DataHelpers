using System.Linq.Expressions;
using PagingFiltering.Helpers.Models;

namespace PagingFiltering.Helpers.Extensions;

/// <summary>
/// Métodos de extensão para aplicação de Keyset (Seek Method) Pagination em coleções em memória e consultas IQueryable.
/// </summary>
public static class KeysetPaginationExtensions
{
    /// <summary>
    /// Aplica paginação por chave (Keyset Seek Method) em uma coleção em memória.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade.</typeparam>
    /// <typeparam name="TKey">O tipo da chave ordenável comparável.</typeparam>
    /// <param name="source">A coleção de dados.</param>
    /// <param name="keySelector">A expressão selecionadora da chave.</param>
    /// <param name="afterKey">A chave limite após a qual os dados devem ser retornados (ou default se for primeira página).</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <param name="ascending">Indica se a ordenação é ascendente (padrão true).</param>
    /// <returns>O resultado paginado via Keyset.</returns>
    public static PagedResultKeyset<T, TKey> ApplyKeyset<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector,
        TKey? afterKey,
        int pageSize,
        bool ascending = true) where TKey : IComparable<TKey>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "O tamanho da página deve ser maior que zero.");

        var ordered = ascending
            ? source.OrderBy(keySelector)
            : source.OrderByDescending(keySelector);

        var filtered = ordered.AsEnumerable();

        if (afterKey != null)
        {
            filtered = ascending
                ? filtered.Where(x => keySelector(x).CompareTo(afterKey) > 0)
                : filtered.Where(x => keySelector(x).CompareTo(afterKey) < 0);
        }

        var pageItems = filtered.Take(pageSize + 1).ToList();
        var hasMore = pageItems.Count > pageSize;

        if (hasMore)
        {
            pageItems.RemoveAt(pageItems.Count - 1);
        }

        var previousKey = pageItems.Count > 0 ? keySelector(pageItems[0]) : default;
        var nextKey = pageItems.Count > 0 ? keySelector(pageItems[^1]) : default;

        return new PagedResultKeyset<T, TKey>(pageItems, pageSize, previousKey, nextKey, hasMore);
    }
}

