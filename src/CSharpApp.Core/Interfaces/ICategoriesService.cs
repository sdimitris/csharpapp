namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Abstraction over the third-party categories catalog.
/// </summary>
public interface ICategoriesService
{
    Task<Result<IReadOnlyCollection<CategoryDto>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
}
