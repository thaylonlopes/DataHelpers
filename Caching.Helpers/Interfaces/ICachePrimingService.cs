namespace Caching.Helpers.Interfaces
{
    public interface ICachePrimingService
    {
        Task PreloadCacheAsync(Dictionary<string, object> dataToPreload);
    }
}
