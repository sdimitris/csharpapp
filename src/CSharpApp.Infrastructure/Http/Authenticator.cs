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
                var error = await response.BuildErrorAsync(cancellationToken);
                logger.LogError("Authentication against the third-party service failed: {Message}", error.Message);
                return Result.Failure<AuthLoginResponse>(error);
            }

            var payload = await response.Content.ReadFromJsonAsync<AuthLoginResponse>(cancellationToken: cancellationToken);

            if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
            {
                return Result.Failure<AuthLoginResponse>(Error.Unexpected("Auth.EmptyToken", "The authentication endpoint returned an empty token."));
            }

            return Result.Success(payload);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            logger.LogError(ex, "Unable to reach the third-party authentication endpoint.");
            return Result.Failure<AuthLoginResponse>(Error.Failure("Auth.Unreachable", "Unable to reach the third-party authentication endpoint."));
        }
    }
}
