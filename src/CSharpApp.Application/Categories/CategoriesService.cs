namespace CSharpApp.Application.Categories;

/// <summary>
/// Domain-facing categories service, mirroring <see cref="CSharpApp.Application.Products.ProductsService"/>
/// (single-category lookups are cached; list results are not).
/// </summary>
public sealed class CategoriesService(ICategoriesApiClient apiClient, ICacheService cache)
    : ICategoriesService
{
    private static readonly TimeSpan ItemExpiration = TimeSpan.FromMinutes(5);

    public Task<Result<IReadOnlyCollection<Category>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
        => apiClient.GetAllAsync(offset, limit, cancellationToken);

    public Task<Result<Category>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.Category(id);
        return cache.GetOrCreateAsync(cacheKey, ItemExpiration, () => apiClient.GetByIdAsync(id, cancellationToken));
    }

    public Task<Result<Category>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        => apiClient.CreateAsync(request, cancellationToken);
}
