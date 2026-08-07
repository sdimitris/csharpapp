using System.Net;
using CSharpApp.Core.Dtos.Auth;
using CSharpApp.Core.Settings;
using CSharpApp.Infrastructure.Http;
using CSharpApp.Tests.Infrastructure.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CSharpApp.Tests.Infrastructure.Http;

public class AuthTokenProviderTests
{
    private static RestApiSettings Settings => new()
    {
        BaseUrl = "https://fake.api/api/v1/",
        Auth = "auth/login",
        Username = "john@mail.com",
        Password = "changeme"
    };

    // Minimal JWT with an "exp" claim far enough in the future for the cache-hit assertions to hold.
    private static string BuildJwt(long expUnixSeconds)
    {
        string Base64UrlEncode(string json) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var header = Base64UrlEncode("""{"alg":"none","typ":"JWT"}""");
        var payload = Base64UrlEncode($$"""{"sub":"1","exp":{{expUnixSeconds}}}""");
        return $"{header}.{payload}.";
    }

    private static (AuthTokenProvider Sut, FakeHttpMessageHandler Handler) CreateSut(AuthLoginResponse response)
    {
        var handler = FakeHttpMessageHandler.ReturningJson(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(Settings.BaseUrl!) };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(HttpClientNames.Auth)).Returns(httpClient);

        var sut = new AuthTokenProvider(factory.Object, Options.Create(Settings), NullLogger<AuthTokenProvider>.Instance);
        return (sut, handler);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ShouldLoginAndReturnToken_OnFirstCall()
    {
        var token = BuildJwt(DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());
        var (sut, handler) = CreateSut(new AuthLoginResponse { AccessToken = token, RefreshToken = "refresh" });

        var result = await sut.GetAccessTokenAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(token, result.Value);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ShouldReuseCachedToken_WhenNotExpired()
    {
        var token = BuildJwt(DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());
        var (sut, handler) = CreateSut(new AuthLoginResponse { AccessToken = token, RefreshToken = "refresh" });

        await sut.GetAccessTokenAsync();
        await sut.GetAccessTokenAsync();
        await sut.GetAccessTokenAsync();

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task InvalidateToken_ShouldForceNewLogin_OnNextCall()
    {
        var token = BuildJwt(DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());
        var (sut, handler) = CreateSut(new AuthLoginResponse { AccessToken = token, RefreshToken = "refresh" });

        await sut.GetAccessTokenAsync();
        sut.InvalidateToken();
        await sut.GetAccessTokenAsync();

        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ShouldReturnFailure_WhenLoginFails()
    {
        var handler = FakeHttpMessageHandler.ReturningStatus(HttpStatusCode.Unauthorized, "invalid credentials");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(Settings.BaseUrl!) };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(HttpClientNames.Auth)).Returns(httpClient);

        var sut = new AuthTokenProvider(factory.Object, Options.Create(Settings), NullLogger<AuthTokenProvider>.Instance);

        var result = await sut.GetAccessTokenAsync();

        Assert.True(result.IsFailure);
    }
}
