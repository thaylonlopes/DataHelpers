namespace Caching.Helpers.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, string? region = null);
        Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null);
        Task RemoveAsync(string key, string? region = null);
        Task InvalidateRegionAsync(string region);
    }
}
