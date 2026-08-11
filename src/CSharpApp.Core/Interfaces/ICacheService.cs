namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Cache-aside abstraction shared by application services: reads from the cache when
/// present, otherwise invokes the factory and caches successful results only.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Returns the cached value for <paramref name="key"/> if present; otherwise invokes
    /// <paramref name="factory"/>, caches the result for <paramref name="ttl"/> only when it
    /// succeeded (failures are never cached), and returns it.
    /// </summary>
    Task<Result<T>> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<Task<Result<T>>> factory);
}
