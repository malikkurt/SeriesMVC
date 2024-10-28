﻿namespace SeriesMvc.Services
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string cacheKey);
        Task SetAsync<T>(string cacheKey, T value, TimeSpan expiration);
        Task RemoveAsync(string cacheKey);
    }
}
