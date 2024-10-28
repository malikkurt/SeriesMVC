using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace SeriesMvc.Services
{
    public class RedisCacheService: ICacheService
    {
        private readonly IDistributedCache _distributedCache;

        public RedisCacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T> GetAsync<T>(string cacheKey)
        {
            var cachedData = await _distributedCache.GetStringAsync(cacheKey);
            return cachedData == null ? default : JsonConvert.DeserializeObject<T>(cachedData);
        }

        public async Task SetAsync<T>(string cacheKey, T value, TimeSpan expiration)
        {
            var serializedData = JsonConvert.SerializeObject(value);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            await _distributedCache.SetStringAsync(cacheKey, serializedData, cacheOptions);
        }

        public async Task RemoveAsync(string cacheKey)
        {
            await _distributedCache.RemoveAsync(cacheKey);
        }
    }
}
