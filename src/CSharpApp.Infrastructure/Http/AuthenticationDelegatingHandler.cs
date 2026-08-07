namespace CSharpApp.Infrastructure.Http;

/// <summary>
/// Attaches a JWT bearer token, obtained through <see cref="IAuthTokenProvider"/>, to every
/// outgoing request. If the third-party service rejects the token with a 401, the token is
/// invalidated and the request is retried once with a freshly issued token.
/// </summary>
public sealed class AuthenticationDelegatingHandler(IAuthTokenProvider authTokenProvider, ILogger<AuthenticationDelegatingHandler> logger)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await SendWithTokenAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        logger.LogWarning("Third-party service responded with 401, refreshing token and retrying once.");
        authTokenProvider.InvalidateToken();
        response.Dispose();

        var retryRequest = await CloneRequestAsync(request, cancellationToken);
        return await SendWithTokenAsync(retryRequest, cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content is not null)
        {
            var buffer = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            var clonedContent = new ByteArrayContent(buffer);
            foreach (var header in request.Content.Headers)
            {
                clonedContent.Headers.Add(header.Key, header.Value);
            }

            clone.Content = clonedContent;
        }

        foreach (var header in request.Headers)
        {
            if (!string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }

    private async Task<HttpResponseMessage> SendWithTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenResult = await authTokenProvider.GetAccessTokenAsync(cancellationToken);

        if (tokenResult.IsSuccess)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value);
        }
        else
        {
            logger.LogError("Unable to obtain a third-party access token: {Message}", tokenResult.Error.Message);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
