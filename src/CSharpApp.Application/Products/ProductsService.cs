namespace CSharpApp.Application.Products;

/// <summary>
/// Domain-facing products service: consumes the pure HTTP gateway (<see cref="IProductsApiClient"/>)
/// and adds cross-cutting concerns (currently cache-aside via <see cref="ICacheService"/> for
/// single-product lookups only; list results are not cached, see <see cref="GetAllAsync"/>).
/// Kept separate from <see cref="IProductsApiClient"/> so future business rules
/// (e.g. filtering, enrichment, mapping to a richer domain model) have a home that
/// doesn't touch HTTP/transport code at all.
/// </summary>
public sealed class ProductsService(IProductsApiClient apiClient, ICacheService cacheService)
    : IProductsService
{
    private static readonly TimeSpan ItemExpiration = TimeSpan.FromMinutes(5);

    public Task<Result<IReadOnlyCollection<Product>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
        => apiClient.GetAllAsync(offset, limit, cancellationToken);

    public Task<Result<Product>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.Product(id);
        return cacheService.GetOrCreateAsync(cacheKey, ItemExpiration, () => apiClient.GetByIdAsync(id, cancellationToken));
    }

    public Task<Result<Product>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        => apiClient.CreateAsync(request, cancellationToken);
}
