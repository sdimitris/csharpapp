namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Abstraction over the third-party categories catalog.
/// </summary>
public interface ICategoriesService
{
    Task<Result<IReadOnlyCollection<Category>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);

    Task<Result<Category>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<Category>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
}
