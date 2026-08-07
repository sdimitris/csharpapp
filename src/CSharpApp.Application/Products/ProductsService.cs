namespace CSharpApp.Application.Products;

/// <summary>
/// Domain-facing products service: consumes the pure HTTP gateway (<see cref="IProductsApiClient"/>)
/// and adds cross-cutting concerns (currently cache-aside via <see cref="IMemoryCache"/>).
/// Kept separate from <see cref="IProductsApiClient"/> so future business rules
/// (e.g. filtering, enrichment, mapping to a richer domain model) have a home that
/// doesn't touch HTTP/transport code at all.
/// </summary>
public sealed class ProductsService(IProductsApiClient apiClient, IMemoryCache cache, ILogger<ProductsService> logger)
    : IProductsService
{
    private static readonly TimeSpan ListExpiration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ItemExpiration = TimeSpan.FromMinutes(5);

    public async Task<Result<IReadOnlyCollection<Product>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.ProductsList(GetListVersion(), offset, limit);

        if (cache.TryGetValue(cacheKey, out Result<IReadOnlyCollection<Product>>? cached) && cached is not null)
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

    public async Task<Result<Product>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.Product(id);

        if (cache.TryGetValue(cacheKey, out Result<Product>? cached) && cached is not null)
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

    public async Task<Result<Product>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var result = await apiClient.CreateAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            BumpListVersion();
            logger.LogDebug("Products list cache generation bumped after create");
        }

        return result;
    }

    /// <summary>
    /// Current cache "generation" for product list pages; see <see cref="CacheKeys.ProductsList"/>.
    /// </summary>
    private int GetListVersion()
        => cache.GetOrCreate(CacheKeys.ProductsListVersion, _ => 0);

    private void BumpListVersion()
        => cache.Set(CacheKeys.ProductsListVersion, GetListVersion() + 1);
}
