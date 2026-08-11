namespace CSharpApp.Application.Common;

/// <summary>
/// Wires the Application layer's use-case services into DI. This layer sits between
/// Core (contracts) and Infrastructure (HTTP gateways), adding cross-cutting concerns
/// (caching) without any MediatR/CQRS pipeline — plain services and clients only.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        services.AddScoped<IProductsService, Products.ProductsService>();
        services.AddScoped<ICategoriesService, Categories.CategoriesService>();

        return services;
    }
}
