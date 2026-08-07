namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Pure HTTP gateway to the third-party products catalog. Responsible only for translating
/// calls into HTTP requests and HTTP responses into <see cref="Result{TValue}"/> — no
/// caching, no business rules. Consumed by <see cref="IProductsService"/>, never directly
/// by endpoints.
/// </summary>
public interface IProductsApiClient
{
    Task<Result<IReadOnlyCollection<Product>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);

    Task<Result<Product>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<Product>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
}
