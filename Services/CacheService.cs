using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab01_Grupo1.Services
{
    public class CacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> createItem, TimeSpan? expiration = null)
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (cachedData != null)
            {
                return JsonSerializer.Deserialize<T>(cachedData);
            }

            var data = await createItem();
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(data), options);
            return data;
        }
    }
}