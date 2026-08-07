namespace CSharpApp.Api.Mappings;

/// <summary>
/// Maps the internal, provider-shaped <see cref="Category"/> model to this API's own
/// public <see cref="CategoryResponse"/> contract.
/// </summary>
public static class CategoryMappings
{
    public static CategoryResponse ToResponse(this Category category) => new(
        category.Id ?? 0,
        category.Name ?? string.Empty,
        category.Image ?? string.Empty,
        category.CreationAt,
        category.UpdatedAt);
}
