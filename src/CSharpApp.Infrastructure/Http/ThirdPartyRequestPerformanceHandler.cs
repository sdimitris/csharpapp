namespace CSharpApp.Infrastructure.Http;

/// <summary>
/// Measures and logs the elapsed time of every outgoing call to the third-party service,
/// registered as the outermost handler on each named <see cref="HttpClient"/> so it wraps
/// the full outbound attempt — including any Polly retries/circuit-breaker short-circuits
/// and the JWT bearer-token handshake performed by <see cref="AuthenticationDelegatingHandler"/>.
/// This complements the API's own <c>RequestPerformanceMiddleware</c> (which times the whole
/// inbound HTTP request) by isolating how much of that total was actually spent waiting on
/// the upstream dependency.
/// </summary>
public sealed class ThirdPartyRequestPerformanceHandler(ILogger<ThirdPartyRequestPerformanceHandler> logger) : DelegatingHandler
{
    private const long SlowRequestThresholdMs = 1000;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            Log(request.Method, request.RequestUri, (int)response.StatusCode, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogWarning(
                ex,
                "Third-party {Method} {Uri} failed after {ElapsedMilliseconds}ms.",
                request.Method,
                request.RequestUri,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void Log(HttpMethod method, Uri? uri, int statusCode, long elapsedMilliseconds)
    {
        var logLevel = elapsedMilliseconds >= SlowRequestThresholdMs ? LogLevel.Warning : LogLevel.Information;

        logger.Log(
            logLevel,
            "Third-party {Method} {Uri} responded {StatusCode} in {ElapsedMilliseconds}ms.",
            method,
            uri,
            statusCode,
            elapsedMilliseconds);
    }
}
