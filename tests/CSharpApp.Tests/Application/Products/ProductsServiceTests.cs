using CSharpApp.Application.Common;
using CSharpApp.Application.Products;
using CSharpApp.Core.Common;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Dtos.Requests;
using CSharpApp.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CSharpApp.Tests.Application.Products;

public class ProductsServiceTests
{
    private static ICacheService CreateCache()
        => new CacheService(new MemoryCache(new MemoryCacheOptions()), NullLogger<CacheService>.Instance);

    [Fact]
    public async Task GetAllAsync_ShouldNotCache_AndAlwaysCallApiClient()
    {
        var apiClient = new Mock<IProductsApiClient>();
        apiClient.Setup(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<ProductDto>>([new ProductDto { Id = 1, Title = "A" }]));

        var sut = new ProductsService(apiClient.Object, CreateCache());

        var first = await sut.GetAllAsync();
        var second = await sut.GetAllAsync();

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        apiClient.Verify(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCacheEachIdSeparately()
    {
        var apiClient = new Mock<IProductsApiClient>();
        apiClient.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new ProductDto { Id = 1 }));
        apiClient.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new ProductDto { Id = 2 }));

        var sut = new ProductsService(apiClient.Object, CreateCache());

        await sut.GetByIdAsync(1);
        await sut.GetByIdAsync(2);
        await sut.GetByIdAsync(1);

        apiClient.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        apiClient.Verify(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldNotCache_WhenApiClientFails()
    {
        var apiClient = new Mock<IProductsApiClient>();
        apiClient.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ProductDto>(Error.NotFound("Products.NotFound", "missing")));

        var sut = new ProductsService(apiClient.Object, CreateCache());

        await sut.GetByIdAsync(1);
        await sut.GetByIdAsync(1);

        apiClient.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateAsync_ShouldPassThroughToApiClient()
    {
        var apiClient = new Mock<IProductsApiClient>();
        apiClient.Setup(x => x.CreateAsync(It.IsAny<CreateProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new ProductDto { Id = 1 }));

        var sut = new ProductsService(apiClient.Object, CreateCache());

        var result = await sut.CreateAsync(new CreateProductRequest { Title = "New", Price = 1, Description = "d", CategoryId = 1, Images = ["i"] });

        Assert.True(result.IsSuccess);
        apiClient.Verify(x => x.CreateAsync(It.IsAny<CreateProductRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
