namespace CSharpApp.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static void MapCategoriesEndpoints(this IVersionedEndpointRouteBuilder app)
    {
        const string basePath = "api/v{version:apiVersion}/categories";

        app.MapGet(basePath, async (int? offset, int? limit, ICategoriesService categoriesService, CancellationToken cancellationToken) =>
            {
                var validationError = PaginationValidator.Validate(offset, limit);
                if (validationError is not null)
                {
                    return validationError.ToValidationApiResult();
                }

                var result = await categoriesService.GetAllAsync(offset, limit, cancellationToken);
                return result.Map(categories => categories.Select(c => c.ToResponse()).ToList()).ToApiResult();
            })
            .WithName("GetCategories")
            .WithTags("Categories")
            .HasApiVersion(1.0);

        app.MapGet($"{basePath}/{{id:int}}", async (int id, ICategoriesService categoriesService, CancellationToken cancellationToken) =>
            {
                var result = await categoriesService.GetByIdAsync(id, cancellationToken);
                return result.Map(c => c.ToResponse()).ToApiResult();
            })
            .WithName("GetCategoryById")
            .WithTags("Categories")
            .HasApiVersion(1.0);

        app.MapPost(basePath, async (CreateCategoryApiRequest request, ICategoriesService categoriesService, CancellationToken cancellationToken) =>
            {
                var validationError = request.Validate();
                if (validationError is not null)
                {
                    return validationError.ToValidationApiResult();
                }

                var apiRequest = new CreateCategoryRequest { Name = request.Name, Image = request.Image };
                var result = await categoriesService.CreateAsync(apiRequest, cancellationToken);
                return result.Map(c => c.ToResponse()).ToCreatedApiResult(category => $"api/v1/categories/{category.Id}");
            })
            .WithName("CreateCategory")
            .WithTags("Categories")
            .HasApiVersion(1.0);
    }
}

