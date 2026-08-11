namespace CSharpApp.Infrastructure.Mappings;

/// <summary>
/// Maps between the third-party Platzi Fake Store API's provider-shaped DTOs and this app's
/// own internal DTOs, isolating the provider's shape to the Infrastructure layer. Read models
/// (<c>*FakePlatziDto</c>) map to internal DTOs; internal request models map to the provider's
/// wire-shaped <c>*FakePlatziRequest</c> before being sent.
/// </summary>
internal static class FakePlatziMappings
{
    public static ProductDto ToDto(this ProductFakePlatziDto product) => new()
    {
        Id = product.Id,
        Title = product.Title,
        Price = product.Price,
        Description = product.Description,
        Images = product.Images,
        CreationAt = product.CreationAt,
        UpdatedAt = product.UpdatedAt,
        Category = product.Category?.ToDto()
    };

    public static CategoryDto ToDto(this CategoryFakePlatziDto category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Image = category.Image,
        CreationAt = category.CreationAt,
        UpdatedAt = category.UpdatedAt
    };

    public static AuthTokenDto ToDto(this AuthLoginResponseFakePlatziDto response) => new()
    {
        AccessToken = response.AccessToken,
        RefreshToken = response.RefreshToken
    };

    public static CreateProductFakePlatziRequest ToFakePlatziRequest(this CreateProductRequest request) => new()
    {
        Title = request.Title,
        Price = request.Price,
        Description = request.Description,
        CategoryId = request.CategoryId,
        Images = request.Images
    };

    public static CreateCategoryFakePlatziRequest ToFakePlatziRequest(this CreateCategoryRequest request) => new()
    {
        Name = request.Name,
        Image = request.Image
    };
}
