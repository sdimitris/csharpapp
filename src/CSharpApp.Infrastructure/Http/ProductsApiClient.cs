namespace CSharpApp.Infrastructure.Http;

/// <summary>
/// Typed HTTP client implementation of <see cref="IProductsApiClient"/> backed by
/// <see cref="IHttpClientFactory"/>, so connections/handlers are pooled and reused instead
/// of creating a new <see cref="HttpClient"/> per call. Pure gateway: only responsible for
/// translating calls into HTTP requests/responses, no caching or business rules.
/// </summary>
public sealed class ProductsApiClient(HttpClient httpClient, IOptions<RestApiSettings> settings, ILogger<ProductsApiClient> logger)
    : IProductsApiClient
{
    private readonly RestApiSettings _settings = settings.Value;

    public async Task<Result<IReadOnlyCollection<Product>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(BuildListUrl(_settings.Products!, offset, limit), cancellationToken);
            var result = await response.ToResultAsync<List<Product>>(logger, cancellationToken);

            return result.IsSuccess
                ? Result.Success<IReadOnlyCollection<Product>>(result.Value)
                : Result.Failure<IReadOnlyCollection<Product>>(result.Error);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the products service.");
            return Result.Failure<IReadOnlyCollection<Product>>(
                Error.Failure("Service.Unreachable", "The service is currently unavailable."));
        }
    }

    public async Task<Result<Product>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_settings.Products}/{id}", cancellationToken);
            return await response.ToResultAsync<Product>(logger, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the products service.");
            return Result.Failure<Product>(
                Error.Failure("Service.Unreachable", "The service is currently unavailable."));
        }
    }

    public async Task<Result<Product>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(_settings.Products, request, cancellationToken);
            return await response.ToResultAsync<Product>(logger, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the products service.");
            return Result.Failure<Product>(
                Error.Failure("Service.Unreachable", "The service is currently unavailable."));
        }
    }

    /// <summary>
    /// Appends the third-party API's <c>offset</c>/<c>limit</c> pagination query parameters
    /// (see https://api.escuelajs.co/api/v1/products?offset=0&amp;limit=10) when supplied.
    /// </summary>
    private static string BuildListUrl(string basePath, int? offset, int? limit)
    {
        var query = new List<string>();

        if (offset is not null)
        {
            query.Add($"offset={offset}");
        }

        if (limit is not null)
        {
            query.Add($"limit={limit}");
        }

        return query.Count == 0 ? basePath : $"{basePath}?{string.Join('&', query)}";
    }
}
