namespace CSharpApp.Api.Middleware;

/// <summary>
/// Measures and logs the elapsed time of every HTTP request, so third-party call
/// performance and general API latency can be monitored from the structured logs.
/// </summary>
public sealed class RequestPerformanceMiddleware(RequestDelegate next, ILogger<RequestPerformanceMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var logLevel = elapsedMs switch
            {
                >= 1000 => LogLevel.Warning,
                _ => LogLevel.Information
            };

            logger.Log(
                logLevel,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsedMs);
        }
    }
}
