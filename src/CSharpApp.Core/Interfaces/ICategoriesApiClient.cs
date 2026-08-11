namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Pure HTTP gateway to the third-party categories catalog. Mirrors <see cref="IProductsApiClient"/>.
/// </summary>
public interface ICategoriesApiClient
{
    Task<Result<IReadOnlyCollection<CategoryDto>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
}
