using System.Net;
using CSharpApp.Infrastructure.Http;
using CSharpApp.Tests.Infrastructure.Support;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CSharpApp.Tests.Infrastructure.Http;

public class ThirdPartyRequestPerformanceHandlerTests
{
    private static HttpClient BuildClient(FakeHttpMessageHandler innerHandler, ILogger<ThirdPartyRequestPerformanceHandler> logger)
    {
        var performanceHandler = new ThirdPartyRequestPerformanceHandler(logger)
        {
            InnerHandler = innerHandler
        };

        return new HttpClient(performanceHandler) { BaseAddress = new Uri("https://fake.api/") };
    }

    [Fact]
    public async Task SendAsync_ShouldReturnInnerResponse_Unmodified()
    {
        var innerHandler = FakeHttpMessageHandler.ReturningStatus(HttpStatusCode.OK);
        var client = BuildClient(innerHandler, NullLogger<ThirdPartyRequestPerformanceHandler>.Instance);

        var response = await client.GetAsync("resource");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(innerHandler.Requests);
    }

    [Fact]
    public async Task SendAsync_ShouldLogCompletion_WithMethodAndStatusCode()
    {
        var capturingLogger = new CapturingLogger<ThirdPartyRequestPerformanceHandler>();
        var innerHandler = FakeHttpMessageHandler.ReturningStatus(HttpStatusCode.OK);
        var client = BuildClient(innerHandler, capturingLogger);

        await client.GetAsync("resource");

        var entry = Assert.Single(capturingLogger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Contains("GET", entry.Message);
        Assert.Contains("200", entry.Message);
    }

    [Fact]
    public async Task SendAsync_ShouldRethrow_AndLogWarning_WhenInnerHandlerThrows()
    {
        var capturingLogger = new CapturingLogger<ThirdPartyRequestPerformanceHandler>();
        var innerHandler = new FakeHttpMessageHandler(_ => throw new HttpRequestException("boom"));
        var client = BuildClient(innerHandler, capturingLogger);

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync("resource"));

        var entry = Assert.Single(capturingLogger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("failed", entry.Message);
    }
}
