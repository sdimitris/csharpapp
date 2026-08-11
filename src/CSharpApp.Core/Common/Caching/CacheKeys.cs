namespace CSharpApp.Core.Common.Caching;

/// <summary>
/// Centralizes cache key construction so queries and the commands that must invalidate
/// them stay in sync.
/// </summary>
public static class CacheKeys
{
    public static string Product(int id) => $"products:{id}";

    public static string Category(int id) => $"categories:{id}";
}
