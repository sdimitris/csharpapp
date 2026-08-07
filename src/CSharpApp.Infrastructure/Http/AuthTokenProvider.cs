namespace CSharpApp.Infrastructure.Http;

/// <summary>
/// Logs in against the third-party JWT authentication endpoint and caches the resulting
/// access token in memory until it is close to expiring, refreshing it transparently and
/// in a thread-safe manner (a single in-flight login request is shared by all callers).
/// </summary>
public sealed class AuthTokenProvider : IAuthTokenProvider
{
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(30);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RestApiSettings _settings;
    private readonly ILogger<AuthTokenProvider> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JwtSecurityTokenHandler _jwtHandler = new();

    private string? _cachedToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public AuthTokenProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<RestApiSettings> settings,
        ILogger<AuthTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<string>> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (IsCachedTokenValid())
        {
            return Result.Success(_cachedToken!);
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            
            // If another thread LoginAsync() and released the lock then the waiting thread at this
            //point should check the token again
            if (IsCachedTokenValid())
            {
                return Result.Success(_cachedToken!);
            }

            return await LoginAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void InvalidateToken()
    {
        _cachedToken = null;
        _expiresAt = DateTimeOffset.MinValue;
    }

    private bool IsCachedTokenValid()
        => _cachedToken is not null && DateTimeOffset.UtcNow < _expiresAt - ExpiryBuffer;

    private async Task<Result<string>> LoginAsync(CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientNames.Auth);

            var loginRequest = new AuthLoginRequest
            {
                Email = _settings.Username ?? string.Empty,
                Password = _settings.Password ?? string.Empty
            };

            var response = await httpClient.PostAsJsonAsync(_settings.Auth, loginRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.BuildErrorAsync(cancellationToken);
                _logger.LogError("Authentication against the third-party service failed: {Message}", error.Message);
                return Result.Failure<string>(error);
            }

            var payload = await response.Content.ReadFromJsonAsync<AuthLoginResponse>(cancellationToken: cancellationToken);

            if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
            {
                return Result.Failure<string>(Error.Unexpected("Auth.EmptyToken", "The authentication endpoint returned an empty token."));
            }

            _cachedToken = payload.AccessToken;
            _expiresAt = ResolveExpiry(payload.AccessToken);

            return Result.Success(_cachedToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            _logger.LogError(ex, "Unable to reach the third-party authentication endpoint.");
            return Result.Failure<string>(Error.Failure("Auth.Unreachable", "Unable to reach the third-party authentication endpoint."));
        }
    }

    private DateTimeOffset ResolveExpiry(string accessToken)
    {
        try
        {
            var token = _jwtHandler.ReadJwtToken(accessToken);
            return token.ValidTo == DateTime.MinValue
                ? DateTimeOffset.UtcNow.AddMinutes(15)
                : new DateTimeOffset(token.ValidTo, TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not parse the JWT expiry claim, falling back to a default TTL.");
            return DateTimeOffset.UtcNow.AddMinutes(15);
        }
    }
}
