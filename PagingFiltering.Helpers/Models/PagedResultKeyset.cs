namespace PagingFiltering.Helpers.Models;

/// <summary>
/// Representa o resultado de uma busca paginada baseada em Keyset (Seek Method) para alta performance em grandes volumes de dados.
/// </summary>
/// <typeparam name="T">O tipo da entidade de dados.</typeparam>
/// <typeparam name="TKey">O tipo da chave de busca de cursor.</typeparam>
public class PagedResultKeyset<T, TKey>
{
    /// <summary>
    /// Lista de itens contidos na página atual.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Tamanho máximo da página solicitado.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// A chave do primeiro item desta página (usada para navegar para a página anterior).
    /// </summary>
    public TKey? PreviousKey { get; }

    /// <summary>
    /// A chave do último item desta página (usada para navegar para a próxima página).
    /// </summary>
    public TKey? NextKey { get; }

    /// <summary>
    /// Indica se existem mais registros após esta página.
    /// </summary>
    public bool HasMore { get; }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="PagedResultKeyset{T, TKey}"/>.
    /// </summary>
    public PagedResultKeyset(IEnumerable<T> items, int pageSize, TKey? previousKey, TKey? nextKey, bool hasMore)
    {
        Items = items?.ToList() ?? new List<T>();
        PageSize = pageSize;
        PreviousKey = previousKey;
        NextKey = nextKey;
        HasMore = hasMore;
    }
}

