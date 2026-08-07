namespace CSharpApp.Api.Middleware;

public static class RequestPerformanceMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestPerformanceLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestPerformanceMiddleware>();
}
