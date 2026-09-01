using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace Caching.Helpers
{
    public class SQLiteCacheService : ICacheService
    {
        private readonly SqliteConnection _connection;

        public SQLiteCacheService()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var command = _connection.CreateCommand();
            command.CommandText = "CREATE TABLE Cache (Key TEXT PRIMARY KEY, Value BLOB, Expiration INTEGER)";
            command.ExecuteNonQuery();
        }

        public async Task<T?> GetAsync<T>(string key, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT Value FROM Cache WHERE Key = $key AND (Expiration IS NULL OR Expiration > $currentTime)";
            command.Parameters.AddWithValue("$key", cacheKey);
            command.Parameters.AddWithValue("$currentTime", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            var result = await command.ExecuteScalarAsync();
            if (result == null) return default;

            var jsonData = CompressionHelper.Decompress((byte[])result);
            T? deserialized = JsonSerializer.Deserialize<T>(jsonData);
            return deserialized;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var jsonData = JsonSerializer.Serialize(value);
            var compressedData = CompressionHelper.Compress(jsonData);

            var command = _connection.CreateCommand();
            command.CommandText = "INSERT OR REPLACE INTO Cache (Key, Value, Expiration) VALUES ($key, $value, $expiration)";
            command.Parameters.AddWithValue("$key", cacheKey);
            command.Parameters.AddWithValue("$value", compressedData);
            command.Parameters.AddWithValue("$expiration", slidingExpiration ? (object)DBNull.Value : DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds());

            await command.ExecuteNonQueryAsync();
        }

        public async Task RemoveAsync(string key, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM Cache WHERE Key = $key";
            command.Parameters.AddWithValue("$key", cacheKey);

            await command.ExecuteNonQueryAsync();
        }

        public async Task InvalidateRegionAsync(string region)
        {
            var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM Cache WHERE Key LIKE $region";
            command.Parameters.AddWithValue("$region", $"{region}:%");

            await command.ExecuteNonQueryAsync();
        }
    }
}
