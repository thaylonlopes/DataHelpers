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

        if (Nullable.GetUnderlyingType(typeof(TKey)) != null)
        {
            throw new InvalidOperationException("Colunas da chave de busca (Keyset Seek) devem ser obrigatoriamente NOT NULL para garantir determinismo SQL.");
        }

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

    /// <summary>
    /// Aplica paginação por chave composta mista (ex: Data DESC, Id ASC) em uma coleção em memória.
    /// </summary>
    public static IEnumerable<T> ApplyKeysetComposite<T, TKey1, TKey2>(
        this IEnumerable<T> source,
        Func<T, TKey1> primaryKeySelector,
        bool primaryAscending,
        Func<T, TKey2> secondaryKeySelector,
        bool secondaryAscending,
        (TKey1 Key1, TKey2 Key2)? afterCursor,
        int pageSize)
        where TKey1 : IComparable<TKey1>
        where TKey2 : IComparable<TKey2>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(primaryKeySelector);
        ArgumentNullException.ThrowIfNull(secondaryKeySelector);
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "O tamanho da página deve ser maior que zero.");

        if (Nullable.GetUnderlyingType(typeof(TKey1)) != null || Nullable.GetUnderlyingType(typeof(TKey2)) != null)
        {
            throw new InvalidOperationException("Colunas da chave de busca (Keyset Seek) devem ser obrigatoriamente NOT NULL para garantir determinismo SQL.");
        }

        var ordered = primaryAscending
            ? source.OrderBy(primaryKeySelector)
            : source.OrderByDescending(primaryKeySelector);

        var fullyOrdered = secondaryAscending
            ? ordered.ThenBy(secondaryKeySelector)
            : ordered.ThenByDescending(secondaryKeySelector);

        var filtered = fullyOrdered.AsEnumerable();

        if (afterCursor != null)
        {
            var cursor = afterCursor.Value;
            filtered = filtered.Where(x =>
            {
                var cmp1 = primaryKeySelector(x).CompareTo(cursor.Key1);
                if (primaryAscending)
                {
                    if (cmp1 > 0) return true;
                    if (cmp1 < 0) return false;
                }
                else
                {
                    if (cmp1 < 0) return true;
                    if (cmp1 > 0) return false;
                }

                var cmp2 = secondaryKeySelector(x).CompareTo(cursor.Key2);
                return secondaryAscending ? cmp2 > 0 : cmp2 < 0;
            });
        }

        return filtered.Take(pageSize);
    }
}

