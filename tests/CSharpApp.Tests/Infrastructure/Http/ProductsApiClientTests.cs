using System.Net;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Dtos.Requests;
using CSharpApp.Core.Settings;
using CSharpApp.Infrastructure.Http;
using CSharpApp.Tests.Infrastructure.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CSharpApp.Tests.Infrastructure.Http;

public class ProductsApiClientTests
{
    private static RestApiSettings Settings => new()
    {
        BaseUrl = "https://fake.api/api/v1/",
        Products = "products",
        Categories = "categories",
        Auth = "auth/login",
        Username = "user",
        Password = "pass"
    };

    private static ProductsApiClient CreateSut(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(Settings.BaseUrl!) };
        return new ProductsApiClient(httpClient, Options.Create(Settings), NullLogger<ProductsApiClient>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccess_WhenApiRespondsOk()
    {
        var products = new List<Product> { new() { Id = 1, Title = "Product 1" } };
        var handler = FakeHttpMessageHandler.ReturningJson(products);
        var sut = CreateSut(handler);

        var result = await sut.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Product 1", result.Value.First().Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenApiRespondsWith404()
    {
        var handler = FakeHttpMessageHandler.ReturningStatus(HttpStatusCode.NotFound, "Product not found");
        var sut = CreateSut(handler);

        var result = await sut.GetByIdAsync(999);

        Assert.True(result.IsFailure);
        Assert.Equal(Core.Common.ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task CreateAsync_ShouldPostToProductsPath_AndReturnCreatedProduct()
    {
        var created = new Product { Id = 10, Title = "New" };
        var handler = FakeHttpMessageHandler.ReturningJson(created, HttpStatusCode.Created);
        var sut = CreateSut(handler);

        var result = await sut.CreateAsync(new CreateProductRequest { Title = "New", Price = 10, Description = "d", CategoryId = 1, Images = ["img"] });

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Id);
        Assert.Single(handler.Requests);
        Assert.EndsWith("products", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFailure_WhenHttpClientThrows()
    {
        var handler = new FakeHttpMessageHandler(_ => throw new HttpRequestException("network down"));
        var sut = CreateSut(handler);

        var result = await sut.GetAllAsync();

        Assert.True(result.IsFailure);
        Assert.Equal(Core.Common.ErrorType.Failure, result.Error.Type);
    }

    [Fact]
    public async Task GetAllAsync_ShouldAppendOffsetAndLimit_WhenSupplied()
    {
        var products = new List<Product> { new() { Id = 1, Title = "Product 1" } };
        var handler = FakeHttpMessageHandler.ReturningJson(products);
        var sut = CreateSut(handler);

        var result = await sut.GetAllAsync(offset: 0, limit: 10);

        Assert.True(result.IsSuccess);
        Assert.Single(handler.Requests);
        Assert.EndsWith("products?offset=0&limit=10", handler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task GetAllAsync_ShouldNotAppendQueryString_WhenOffsetAndLimitAreNull()
    {
        var products = new List<Product> { new() { Id = 1, Title = "Product 1" } };
        var handler = FakeHttpMessageHandler.ReturningJson(products);
        var sut = CreateSut(handler);

        await sut.GetAllAsync();

        Assert.Single(handler.Requests);
        Assert.EndsWith("products", handler.Requests[0].RequestUri!.ToString());
    }
}
