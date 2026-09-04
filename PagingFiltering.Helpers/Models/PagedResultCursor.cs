namespace PagingFiltering.Helpers.Models
{
    public class PagedResultCursor<T>
    {
        public List<T> Items { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public long TotalItems { get; set; }
        public string NextCursor { get; set; }

        public PagedResultCursor(List<T> items, int pageSize, long totalItems, string nextCursor)
        {
            Items = items;
            PageSize = pageSize;
            TotalItems = totalItems;
            NextCursor = nextCursor;
            CurrentPage = (int)Math.Ceiling((double)totalItems / pageSize);
        }
    }
}