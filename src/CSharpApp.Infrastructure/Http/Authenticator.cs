namespace CSharpApp.Infrastructure.Http;

/// <summary>
/// Logs in against the third-party JWT authentication endpoint using the configured
/// credentials. Purely responsible for the login HTTP call — caching, expiry tracking,
/// and concurrency control live in <see cref="AuthTokenProvider"/>.
/// </summary>
public sealed class Authenticator(
    IHttpClientFactory httpClientFactory,
    IOptions<RestApiSettings> settings,
    ILogger<Authenticator> logger) : IAuthenticator
{
    private readonly RestApiSettings _settings = settings.Value;

    public async Task<Result<AuthLoginResponse>> LoginAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(HttpClientNames.Auth);

            var loginRequest = new AuthLoginRequest
            {
                Email = _settings.Username ?? string.Empty,
                Password = _settings.Password ?? string.Empty
            };

            var response = await httpClient.PostAsJsonAsync(_settings.Auth, loginRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.BuildErrorAsync(logger, cancellationToken);
                return Result.Failure<AuthLoginResponse>(error);
            }

            var payload = await response.Content.ReadFromJsonAsync<AuthLoginResponse>(cancellationToken: cancellationToken);

            if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
            {
                logger.LogError("Authentication succeeded but returned an empty token.");
                return Result.Failure<AuthLoginResponse>(Error.Unexpected("Auth.EmptyToken", "Authentication is currently unavailable."));
            }

            return Result.Success(payload);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the authentication service.");
            return Result.Failure<AuthLoginResponse>(Error.Failure("Auth.Unreachable", "Authentication is currently unavailable."));
        }
    }
}
