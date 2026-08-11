using CSharpApp.Api.Middleware;
using CSharpApp.Tests.Infrastructure.Support;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CSharpApp.Tests.Api.Middleware;

public class RequestPerformanceMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldLogTheFinalStatusCode_SetByDownstreamMiddleware()
    {
        var logger = new CapturingLogger<RequestPerformanceMiddleware>();
        var middleware = new RequestPerformanceMiddleware(
            context =>
            {
                // Simulates a downstream middleware (e.g. the exception handler) resolving
                // the response before control returns to this middleware's `finally` block.
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            },
            logger);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/products";

        await middleware.InvokeAsync(context);

        var entry = Assert.Single(logger.Entries);
        Assert.Contains("GET", entry.Message);
        Assert.Contains("404", entry.Message);
    }

    [Fact]
    public async Task InvokeAsync_ShouldStillLog_WhenNextThrowsAnUnhandledException()
    {
        var logger = new CapturingLogger<RequestPerformanceMiddleware>();
        var middleware = new RequestPerformanceMiddleware(
            _ => throw new InvalidOperationException("boom"),
            logger);

        var context = new DefaultHttpContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

        // The `finally` block always logs, regardless of the outcome, so no request goes
        // unmeasured even if it later blows up further up the pipeline.
        Assert.Single(logger.Entries);
    }
}
