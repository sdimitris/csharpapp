namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Pure HTTP gateway to the third-party categories catalog. Mirrors <see cref="IProductsApiClient"/>.
/// </summary>
public interface ICategoriesApiClient
{
    Task<Result<IReadOnlyCollection<Category>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);

    Task<Result<Category>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<Category>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
}
