namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Pure HTTP gateway to the third-party products catalog. Responsible only for translating
/// calls into HTTP requests and HTTP responses into <see cref="Result{TValue}"/> — no
/// caching, no business rules. Consumed by <see cref="IProductsService"/>, never directly
/// by endpoints.
/// </summary>
public interface IProductsApiClient
{
    Task<Result<IReadOnlyCollection<ProductDto>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);

    Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
}
