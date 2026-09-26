namespace PagingFiltering.Helpers.Models;

/// <summary>
/// Representa o resultado de uma consulta paginada baseada em cursor (Keyset Seek).
/// </summary>
/// <typeparam name="T">O tipo dos itens contidos na página.</typeparam>
public class PagedResultCursor<T>
{
    /// <summary>
    /// Lista dos itens retornados na página atual.
    /// </summary>
    public List<T> Items { get; set; }

    /// <summary>
    /// A quantidade máxima de itens por página solicitada.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Estimativa da página atual calculada com base na contagem e tamanho de página.
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Contagem total de itens correspondentes.
    /// </summary>
    public long TotalItems { get; set; }

    /// <summary>
    /// O token ou valor de cursor a ser fornecido na próxima requisição para continuar a navegação.
    /// </summary>
    public string NextCursor { get; set; }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="PagedResultCursor{T}"/>.
    /// </summary>
    /// <param name="items">Coleção de itens da página.</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <param name="totalItems">Total geral de registros disponíveis.</param>
    /// <param name="nextCursor">Valor do próximo cursor de paginação.</param>
    public PagedResultCursor(List<T> items, int pageSize, long totalItems, string nextCursor)
    {
        Items = items;
        PageSize = pageSize;
        TotalItems = totalItems;
        NextCursor = nextCursor;
        CurrentPage = (int)Math.Ceiling((double)totalItems / pageSize);
    }
}