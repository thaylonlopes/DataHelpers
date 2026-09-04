namespace PagingFiltering.Helpers.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string cacheKey);
        Task SetAsync<T>(string cacheKey, T item, TimeSpan expiration);
        Task SetCachedDataAsync<T>(string cacheKey, T data);
    }
}
