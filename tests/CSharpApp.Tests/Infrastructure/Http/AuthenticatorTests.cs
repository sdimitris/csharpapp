using System.Net;
using CSharpApp.Core.Settings;
using CSharpApp.Infrastructure.Configuration;
using CSharpApp.Infrastructure.Http;
using CSharpApp.Infrastructure.Http.Dtos;
using CSharpApp.Tests.Infrastructure.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CSharpApp.Tests.Infrastructure.Http;

public class AuthenticatorTests
{
    private static RestApiSettings Settings => new()
    {
        BaseUrl = "https://fake.api/api/v1/",
        Auth = "auth/login",
        Username = "john@mail.com",
        Password = "changeme"
    };

    private static (Authenticator Sut, FakeHttpMessageHandler Handler) CreateSut(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(Settings.BaseUrl!) };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(HttpClientNames.Auth)).Returns(httpClient);

        var sut = new Authenticator(factory.Object, Options.Create(Settings), NullLogger<Authenticator>.Instance);
        return (sut, handler);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAccessToken_WhenLoginSucceeds()
    {
        var response = new AuthLoginResponseFakePlatziDto { AccessToken = "token", RefreshToken = "refresh" };
        var (sut, handler) = CreateSut(FakeHttpMessageHandler.ReturningJson(response));

        var result = await sut.LoginAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("token", result.Value.AccessToken);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenCredentialsAreRejected()
    {
        var (sut, _) = CreateSut(FakeHttpMessageHandler.ReturningStatus(HttpStatusCode.Unauthorized, "invalid credentials"));

        var result = await sut.LoginAsync();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenResponseHasNoAccessToken()
    {
        var response = new AuthLoginResponseFakePlatziDto { AccessToken = string.Empty, RefreshToken = "refresh" };
        var (sut, _) = CreateSut(FakeHttpMessageHandler.ReturningJson(response));

        var result = await sut.LoginAsync();

        Assert.True(result.IsFailure);
    }
}
