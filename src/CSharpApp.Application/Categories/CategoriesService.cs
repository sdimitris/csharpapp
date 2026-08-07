namespace CSharpApp.Application.Categories;

/// <summary>
/// Domain-facing categories service, mirroring <see cref="CSharpApp.Application.Products.ProductsService"/>.
/// </summary>
public sealed class CategoriesService(ICategoriesApiClient apiClient, IMemoryCache cache, ILogger<CategoriesService> logger)
    : ICategoriesService
{
    private static readonly TimeSpan ListExpiration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ItemExpiration = TimeSpan.FromMinutes(5);

    public async Task<Result<IReadOnlyCollection<Category>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.CategoriesList(GetListVersion(), offset, limit);

        if (cache.TryGetValue(cacheKey, out Result<IReadOnlyCollection<Category>>? cached) && cached is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        var result = await apiClient.GetAllAsync(offset, limit, cancellationToken);

        if (result.IsSuccess)
        {
            cache.Set(cacheKey, result, ListExpiration);
            logger.LogDebug("Cache populated for {CacheKey}", cacheKey);
        }

        return result;
    }

    public async Task<Result<Category>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.Category(id);

        if (cache.TryGetValue(cacheKey, out Result<Category>? cached) && cached is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        var result = await apiClient.GetByIdAsync(id, cancellationToken);

        if (result.IsSuccess)
        {
            cache.Set(cacheKey, result, ItemExpiration);
            logger.LogDebug("Cache populated for {CacheKey}", cacheKey);
        }

        return result;
    }

    public async Task<Result<Category>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await apiClient.CreateAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            BumpListVersion();
            logger.LogDebug("Categories list cache generation bumped after create");
        }

        return result;
    }

    /// <summary>
    /// Current cache "generation" for category list pages; see <see cref="CacheKeys.CategoriesList"/>.
    /// </summary>
    private int GetListVersion()
        => cache.GetOrCreate(CacheKeys.CategoriesListVersion, _ => 0);

    private void BumpListVersion()
        => cache.Set(CacheKeys.CategoriesListVersion, GetListVersion() + 1);
}
