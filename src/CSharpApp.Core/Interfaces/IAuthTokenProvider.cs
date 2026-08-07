namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Provides a valid JWT access token for authenticating outgoing requests to the
/// third-party service, transparently handling login and token refresh/caching.
/// </summary>
public interface IAuthTokenProvider
{
    Task<Result<string>> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces the next call to <see cref="GetAccessTokenAsync"/> to fetch a fresh token,
    /// used when the third-party service rejects the currently cached token.
    /// </summary>
    void InvalidateToken();
}
