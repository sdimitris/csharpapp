namespace CSharpApp.Infrastructure.Http;

/// <summary>
/// Typed HTTP client implementation of <see cref="ICategoriesApiClient"/>. Pure gateway,
/// mirrors <see cref="ProductsApiClient"/>.
/// </summary>
public sealed class CategoriesApiClient(HttpClient httpClient, IOptions<RestApiSettings> settings, ILogger<CategoriesApiClient> logger)
    : ICategoriesApiClient
{
    private readonly RestApiSettings _settings = settings.Value;

    public async Task<Result<IReadOnlyCollection<CategoryDto>>> GetAllAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(BuildListUrl(_settings.Categories!, offset, limit), cancellationToken);
            var result = await response.ToResultAsync<List<CategoryFakePlatziDto>>(logger, cancellationToken);

            return result.IsSuccess
                ? Result.Success<IReadOnlyCollection<CategoryDto>>(result.Value.Select(c => c.ToDto()).ToList())
                : Result.Failure<IReadOnlyCollection<CategoryDto>>(result.Error);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the categories service.");
            return Result.Failure<IReadOnlyCollection<CategoryDto>>(
                Error.Failure("Service.Unreachable", "The service is currently unavailable."));
        }
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_settings.Categories}/{id}", cancellationToken);
            var result = await response.ToResultAsync<CategoryFakePlatziDto>(logger, cancellationToken);

            return result.IsSuccess
                ? Result.Success(result.Value.ToDto())
                : Result.Failure<CategoryDto>(result.Error);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the categories service.");
            return Result.Failure<CategoryDto>(
                Error.Failure("Service.Unreachable", "The service is currently unavailable."));
        }
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(_settings.Categories, request.ToFakePlatziRequest(), cancellationToken);
            var result = await response.ToResultAsync<CategoryFakePlatziDto>(logger, cancellationToken);

            return result.IsSuccess
                ? Result.Success(result.Value.ToDto())
                : Result.Failure<CategoryDto>(result.Error);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the categories service.");
            return Result.Failure<CategoryDto>(
                Error.Failure("Service.Unreachable", "The service is currently unavailable."));
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
