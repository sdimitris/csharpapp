namespace CSharpApp.Api.Endpoints;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this IVersionedEndpointRouteBuilder app)
    {
        const string basePath = "api/v{version:apiVersion}/products";

        app.MapGet(basePath, async (int? offset, int? limit, IProductsService productsService, CancellationToken cancellationToken) =>
            {
                var validationError = PaginationValidator.Validate(offset, limit);
                if (validationError is not null)
                {
                    return validationError.ToValidationApiResult();
                }

                var result = await productsService.GetAllAsync(offset, limit, cancellationToken);
                return result.Map(products => products.Select(p => p.ToResponse()).ToList()).ToApiResult();
            })
            .WithName("GetProducts")
            .WithTags("Products")
            .HasApiVersion(1.0);

        app.MapGet($"{basePath}/{{id:int}}", async (int id, IProductsService productsService, CancellationToken cancellationToken) =>
            {
                var result = await productsService.GetByIdAsync(id, cancellationToken);
                return result.Map(p => p.ToResponse()).ToApiResult();
            })
            .WithName("GetProductById")
            .WithTags("Products")
            .HasApiVersion(1.0);

        app.MapPost(basePath, async (CreateProductApiRequest request, IProductsService productsService, CancellationToken cancellationToken) =>
            {
                var validationError = request.Validate();
                if (validationError is not null)
                {
                    return validationError.ToValidationApiResult();
                }

                var apiRequest = new CreateProductRequest
                {
                    Title = request.Title,
                    Price = request.Price,
                    Description = request.Description,
                    CategoryId = request.CategoryId,
                    Images = request.Images
                };

                var result = await productsService.CreateAsync(apiRequest, cancellationToken);
                return result.Map(p => p.ToResponse()).ToCreatedApiResult(product => $"api/v1/products/{product.Id}");
            })
            .WithName("CreateProduct")
            .WithTags("Products")
            .HasApiVersion(1.0);
    }
}

