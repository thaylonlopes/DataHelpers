using Caching.Helpers;

Console.WriteLine("=== Demo: TL.Caching.Helpers with SQLiteCacheService ===");

var cache = new SQLiteCacheService();
await cache.SetAsync("key1", "value1", TimeSpan.FromMinutes(5));
var value = await cache.GetAsync<string>("key1");
Console.WriteLine($"Cached value: {value}");
