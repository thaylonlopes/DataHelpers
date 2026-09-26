namespace PagingFiltering.Helpers.Models;

/// <summary>
/// Representa o resultado de uma consulta paginada tradicional baseada em deslocamento (offset).
/// </summary>
/// <typeparam name="T">O tipo dos itens contidos na página.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Lista dos itens retornados na página atual.
    /// </summary>
    public List<T> Items { get; set; }

    /// <summary>
    /// O número da página atual (índice 1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// A quantidade máxima de itens por página solicitada.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// A contagem total de itens correspondentes ao filtro antes da paginação.
    /// </summary>
    public long TotalItems { get; set; }

    /// <summary>
    /// A quantidade total de páginas calculadas com base no total de itens e no tamanho da página.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="PagedResult{T}"/>.
    /// </summary>
    /// <param name="items">Coleção de itens da página.</param>
    /// <param name="pageNumber">Número da página atual.</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <param name="totalItems">Total geral de registros disponíveis.</param>
    public PagedResult(List<T> items, int pageNumber, int pageSize, long totalItems)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }
}
