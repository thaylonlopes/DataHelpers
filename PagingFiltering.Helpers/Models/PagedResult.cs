namespace PagingFiltering.Helpers.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long TotalItems { get; set; }
        public int TotalPages { get; set; }

        public PagedResult(List<T> items, int pageNumber, int pageSize, long totalItems)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        }
    }
}
