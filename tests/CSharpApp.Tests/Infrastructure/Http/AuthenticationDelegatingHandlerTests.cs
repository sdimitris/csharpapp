using System.Net;
using CSharpApp.Core.Common;
using CSharpApp.Core.Interfaces;
using CSharpApp.Infrastructure.Http;
using CSharpApp.Tests.Infrastructure.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CSharpApp.Tests.Infrastructure.Http;

public class AuthenticationDelegatingHandlerTests
{
    private static HttpClient BuildClient(IAuthTokenProvider tokenProvider, FakeHttpMessageHandler innerHandler)
    {
        var authHandler = new AuthenticationDelegatingHandler(tokenProvider, NullLogger<AuthenticationDelegatingHandler>.Instance)
        {
            InnerHandler = innerHandler
        };

        return new HttpClient(authHandler) { BaseAddress = new Uri("https://fake.api/") };
    }

    [Fact]
    public async Task SendAsync_ShouldAttachBearerToken_ToOutgoingRequest()
    {
        var tokenProvider = new Mock<IAuthTokenProvider>();
        tokenProvider.Setup(t => t.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("token-123"));

        var innerHandler = FakeHttpMessageHandler.ReturningStatus(HttpStatusCode.OK);
        var client = BuildClient(tokenProvider.Object, innerHandler);

        await client.GetAsync("resource");

        Assert.Equal("Bearer", innerHandler.Requests[0].Headers.Authorization!.Scheme);
        Assert.Equal("token-123", innerHandler.Requests[0].Headers.Authorization!.Parameter);
    }

    [Fact]
    public async Task SendAsync_ShouldInvalidateTokenAndRetryOnce_When401Received()
    {
        var tokenProvider = new Mock<IAuthTokenProvider>();
        tokenProvider.Setup(t => t.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("token-123"));

        var callCount = 0;
        var innerHandler = new FakeHttpMessageHandler(_ =>
        {
            callCount++;
            return new HttpResponseMessage(callCount == 1 ? HttpStatusCode.Unauthorized : HttpStatusCode.OK);
        });

        var client = BuildClient(tokenProvider.Object, innerHandler);

        var response = await client.GetAsync("resource");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, innerHandler.Requests.Count);
        tokenProvider.Verify(t => t.InvalidateToken(), Times.Once);
    }
}
