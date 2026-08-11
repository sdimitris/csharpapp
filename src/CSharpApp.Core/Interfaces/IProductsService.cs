namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Abstraction over the third-party products catalog. Implementations are expected to
/// return a <see cref="Result{TValue}"/> so callers never need to rely on exceptions
/// for expected failure modes (not found, transient network failures, etc.).
/// </summary>
public interface IProductsService
{
    Task<Result<IReadOnlyCollection<ProductDto>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);

    Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
}