namespace CSharpApp.Application.Common;

/// <summary>
/// <see cref="ICacheService"/> implementation backed by <see cref="IMemoryCache"/>. Centralizes
/// the cache-aside mechanics (including hit/miss logging and versioned list invalidation)
/// previously duplicated between <see cref="Products.ProductsService"/> and
/// <see cref="Categories.CategoriesService"/>.
/// </summary>
public sealed class CacheService(IMemoryCache cache, ILogger<CacheService> logger) : ICacheService
{
    public async Task<Result<T>> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<Task<Result<T>>> factory)
    {
        if (cache.TryGetValue(key, out Result<T>? cached) && cached is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", key);
            return cached;
        }

        var result = await factory();

        if (result.IsSuccess)
        {
            cache.Set(key, result, ttl);
            logger.LogDebug("Cache populated for {CacheKey}", key);
        }

        return result;
    }
}
