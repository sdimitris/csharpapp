namespace CSharpApp.Core.Common.Caching;

/// <summary>
/// Centralizes cache key construction so queries and the commands that must invalidate
/// them stay in sync.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Tracks a "generation" for each list cache, incremented whenever a create bypasses
    /// caches for that resource. Since <see cref="System.Runtime.Caching"/>-style stores have
    /// no built-in wildcard eviction, every paginated list cache entry (one per distinct
    /// offset/limit combination) embeds the current generation in its key. Bumping the
    /// generation instantly makes all previously cached pages unreachable (they simply
    /// expire naturally afterwards) without needing to enumerate/remove every page.
    /// </summary>
    public const string ProductsListVersion = "products:list-version";

    public const string CategoriesListVersion = "categories:list-version";

    public static string ProductsList(int version, int? offset, int? limit)
        => $"products:all:v{version}:o={offset?.ToString() ?? "-"}:l={limit?.ToString() ?? "-"}";

    public static string CategoriesList(int version, int? offset, int? limit)
        => $"categories:all:v{version}:o={offset?.ToString() ?? "-"}:l={limit?.ToString() ?? "-"}";

    public static string Product(int id) => $"products:{id}";

    public static string Category(int id) => $"categories:{id}";
}
