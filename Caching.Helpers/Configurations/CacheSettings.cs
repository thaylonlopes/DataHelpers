namespace Caching.Helpers.Configurations
{
    public class CacheSettings
    {
        public required string RedisConnectionString { get; set; } = null!;
        public required string MemcachedServer { get; set; } = null!;

    }
}
