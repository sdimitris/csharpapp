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
    [Fact]
    public async Task GetAllAsync_ShouldCallApiClientOnce_WhenCalledTwiceBeforeExpiration()
    {
        var apiClient = new Mock<IProductsApiClient>();
        apiClient.Setup(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<Product>>([new Product { Id = 1, Title = "A" }]));

        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ProductsService(apiClient.Object, cache, NullLogger<ProductsService>.Instance);

        var first = await sut.GetAllAsync();
        var second = await sut.GetAllAsync();

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        apiClient.Verify(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldNotCache_WhenApiClientFails()
    {
        var apiClient = new Mock<IProductsApiClient>();
        apiClient.Setup(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IReadOnlyCollection<Product>>(Error.Failure("Products.Unreachable", "down")));

        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ProductsService(apiClient.Object, cache, NullLogger<ProductsService>.Instance);

        await sut.GetAllAsync();
        await sut.GetAllAsync();

        apiClient.Verify(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCacheEachIdSeparately()
    {
        var apiClient = new Mock<IProductsApiClient>();
        apiClient.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new Product { Id = 1 }));
        apiClient.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new Product { Id = 2 }));

        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ProductsService(apiClient.Object, cache, NullLogger<ProductsService>.Instance);

        await sut.GetByIdAsync(1);
        await sut.GetByIdAsync(2);
        await sut.GetByIdAsync(1);

        apiClient.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        apiClient.Verify(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldInvalidateAllProductsCache_OnSuccess()
    {
        var apiClient = new Mock<IProductsApiClient>();
        apiClient.SetupSequence(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<Product>>([new Product { Id = 1 }]))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<Product>>([new Product { Id = 1 }, new Product { Id = 2 }]));
        apiClient.Setup(x => x.CreateAsync(It.IsAny<CreateProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new Product { Id = 2 }));

        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ProductsService(apiClient.Object, cache, NullLogger<ProductsService>.Instance);

        await sut.GetAllAsync();
        await sut.CreateAsync(new CreateProductRequest { Title = "New", Price = 1, Description = "d", CategoryId = 1, Images = ["i"] });
        var afterCreate = await sut.GetAllAsync();

        Assert.Equal(2, afterCreate.Value.Count);
        apiClient.Verify(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAllAsync_ShouldCacheByOffsetAndLimit_Separately()
    {
        var apiClient = new Mock<IProductsApiClient>();
        apiClient.Setup(x => x.GetAllAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<Product>>([new Product { Id = 1 }]));
        apiClient.Setup(x => x.GetAllAsync(10, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyCollection<Product>>([new Product { Id = 2 }]));

        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ProductsService(apiClient.Object, cache, NullLogger<ProductsService>.Instance);

        await sut.GetAllAsync(0, 10);
        await sut.GetAllAsync(10, 10);
        await sut.GetAllAsync(0, 10);

        apiClient.Verify(x => x.GetAllAsync(0, 10, It.IsAny<CancellationToken>()), Times.Once);
        apiClient.Verify(x => x.GetAllAsync(10, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
