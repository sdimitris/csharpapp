namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Authenticates against the third-party service's login endpoint using the configured
/// credentials. Knows nothing about caching, expiry, or token reuse — that is the
/// responsibility of <see cref="IAuthTokenProvider"/>.
/// </summary>
public interface IAuthenticator
{
    Task<Result<AuthTokenDto>> LoginAsync(CancellationToken cancellationToken = default);
}
