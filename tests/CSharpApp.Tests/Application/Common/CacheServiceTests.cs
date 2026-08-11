using CSharpApp.Application.Common;
using CSharpApp.Core.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CSharpApp.Tests.Application.Common;

public class CacheServiceTests
{
    private static CacheService CreateSut()
        => new(new MemoryCache(new MemoryCacheOptions()), NullLogger<CacheService>.Instance);

    [Fact]
    public async Task GetOrCreateAsync_ShouldCallFactoryOnce_WhenCalledTwiceForSameKey()
    {
        var sut = CreateSut();
        var callCount = 0;

        Task<Result<int>> Factory()
        {
            callCount++;
            return Task.FromResult(Result.Success(42));
        }

        var first = await sut.GetOrCreateAsync("key", TimeSpan.FromMinutes(1), Factory);
        var second = await sut.GetOrCreateAsync("key", TimeSpan.FromMinutes(1), Factory);

        Assert.Equal(42, first.Value);
        Assert.Equal(42, second.Value);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldNotCacheFailures()
    {
        var sut = CreateSut();
        var callCount = 0;

        Task<Result<int>> Factory()
        {
            callCount++;
            return Task.FromResult(Result.Failure<int>(Error.Failure("Test.Failure", "boom")));
        }

        await sut.GetOrCreateAsync("key", TimeSpan.FromMinutes(1), Factory);
        await sut.GetOrCreateAsync("key", TimeSpan.FromMinutes(1), Factory);

        Assert.Equal(2, callCount);
    }
}
