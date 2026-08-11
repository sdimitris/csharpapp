namespace CSharpApp.Infrastructure.Http;

/// <summary>
/// Caches the JWT access token obtained from <see cref="IAuthenticator"/> in memory until it
/// is close to expiring, refreshing it transparently and in a thread-safe manner (a single
/// in-flight login request is shared by all callers).
/// </summary>
public sealed class AuthTokenProvider : IAuthTokenProvider
{
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(30);

    private readonly IAuthenticator _authenticator;
    private readonly ILogger<AuthTokenProvider> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JwtSecurityTokenHandler _jwtHandler = new();

    private string? _cachedToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public AuthTokenProvider(IAuthenticator authenticator, ILogger<AuthTokenProvider> logger)
    {
        _authenticator = authenticator;
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
            
            // If another thread logged in and released the lock then the waiting thread at this
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
        var loginResult = await _authenticator.LoginAsync(cancellationToken);

        if (loginResult.IsFailure)
        {
            return Result.Failure<string>(loginResult.Error);
        }

        _cachedToken = loginResult.Value.AccessToken;
        _expiresAt = ResolveExpiry(_cachedToken);

        return Result.Success(_cachedToken);
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
