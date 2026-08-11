namespace CSharpApp.Api.Mappings;

/// <summary>
/// Maps this app's internal <see cref="CategoryDto"/> to this API's own public
/// <see cref="CategoryResponse"/> contract.
/// </summary>
public static class CategoryMappings
{
    public static CategoryResponse ToResponse(this CategoryDto category) => new(
        category.Id ?? 0,
        category.Name ?? string.Empty,
        category.Image ?? string.Empty,
        category.CreationAt,
        category.UpdatedAt);
}
