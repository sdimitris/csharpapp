using CSharpApp.Core.Common;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Interfaces;
using CSharpApp.Infrastructure.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CSharpApp.Tests.Infrastructure.Http;

public class AuthTokenProviderTests
{
    // Minimal JWT with an "exp" claim far enough in the future for the cache-hit assertions to hold.
    private static string BuildJwt(long expUnixSeconds)
    {
        string Base64UrlEncode(string json) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var header = Base64UrlEncode("""{"alg":"none","typ":"JWT"}""");
        var payload = Base64UrlEncode($$"""{"sub":"1","exp":{{expUnixSeconds}}}""");
        return $"{header}.{payload}.";
    }

    private static (AuthTokenProvider Sut, Mock<IAuthenticator> Authenticator) CreateSut(AuthTokenDto response)
    {
        var authenticator = new Mock<IAuthenticator>();
        authenticator
            .Setup(a => a.LoginAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(response));

        var sut = new AuthTokenProvider(authenticator.Object, NullLogger<AuthTokenProvider>.Instance);
        return (sut, authenticator);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ShouldLoginAndReturnToken_OnFirstCall()
    {
        var token = BuildJwt(DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());
        var (sut, authenticator) = CreateSut(new AuthTokenDto { AccessToken = token, RefreshToken = "refresh" });

        var result = await sut.GetAccessTokenAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(token, result.Value);
        authenticator.Verify(a => a.LoginAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAccessTokenAsync_ShouldReuseCachedToken_WhenNotExpired()
    {
        var token = BuildJwt(DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());
        var (sut, authenticator) = CreateSut(new AuthTokenDto { AccessToken = token, RefreshToken = "refresh" });

        await sut.GetAccessTokenAsync();
        await sut.GetAccessTokenAsync();
        await sut.GetAccessTokenAsync();

        authenticator.Verify(a => a.LoginAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateToken_ShouldForceNewLogin_OnNextCall()
    {
        var token = BuildJwt(DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());
        var (sut, authenticator) = CreateSut(new AuthTokenDto { AccessToken = token, RefreshToken = "refresh" });

        await sut.GetAccessTokenAsync();
        sut.InvalidateToken();
        await sut.GetAccessTokenAsync();

        authenticator.Verify(a => a.LoginAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAccessTokenAsync_ShouldReturnFailure_WhenLoginFails()
    {
        var authenticator = new Mock<IAuthenticator>();
        authenticator
            .Setup(a => a.LoginAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthTokenDto>(Error.Failure("Auth.Unauthorized", "invalid credentials")));

        var sut = new AuthTokenProvider(authenticator.Object, NullLogger<AuthTokenProvider>.Instance);

        var result = await sut.GetAccessTokenAsync();

        Assert.True(result.IsFailure);
    }
}
