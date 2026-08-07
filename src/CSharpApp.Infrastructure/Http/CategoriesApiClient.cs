namespace CSharpApp.Infrastructure.Http;

/// <summary>
/// Typed HTTP client implementation of <see cref="ICategoriesApiClient"/>. Pure gateway,
/// mirrors <see cref="ProductsApiClient"/>.
/// </summary>
public sealed class CategoriesApiClient(HttpClient httpClient, IOptions<RestApiSettings> settings, ILogger<CategoriesApiClient> logger)
    : ICategoriesApiClient
{
    private readonly RestApiSettings _settings = settings.Value;

    public async Task<Result<IReadOnlyCollection<Category>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(BuildListUrl(_settings.Categories!, offset, limit), cancellationToken);
            var result = await response.ToResultAsync<List<Category>>(cancellationToken);

            return result.IsSuccess
                ? Result.Success<IReadOnlyCollection<Category>>(result.Value)
                : Result.Failure<IReadOnlyCollection<Category>>(result.Error);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the third-party categories endpoint.");
            return Result.Failure<IReadOnlyCollection<Category>>(
                Error.Failure("Categories.Unreachable", "Unable to reach the third-party categories endpoint."));
        }
    }

    public async Task<Result<Category>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_settings.Categories}/{id}", cancellationToken);
            return await response.ToResultAsync<Category>(cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the third-party categories endpoint.");
            return Result.Failure<Category>(
                Error.Failure("Categories.Unreachable", "Unable to reach the third-party categories endpoint."));
        }
    }

    public async Task<Result<Category>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(_settings.Categories, request, cancellationToken);
            return await response.ToResultAsync<Category>(cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the third-party categories endpoint.");
            return Result.Failure<Category>(
                Error.Failure("Categories.Unreachable", "Unable to reach the third-party categories endpoint."));
        }
    }

    /// <summary>
    /// Appends the third-party API's <c>offset</c>/<c>limit</c> pagination query parameters
    /// (see https://api.escuelajs.co/api/v1/categories?offset=0&amp;limit=10) when supplied.
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
