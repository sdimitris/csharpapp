namespace CSharpApp.Api.Mappings;

/// <summary>
/// Maps this app's internal <see cref="ProductDto"/> to this API's own public
/// <see cref="ProductResponse"/> contract.
/// </summary>
public static class ProductMappings
{
    public static ProductResponse ToResponse(this ProductDto product) => new(
        product.Id ?? 0,
        product.Title ?? string.Empty,
        product.Price ?? 0,
        product.Description ?? string.Empty,
        product.Images,
        product.CreationAt,
        product.UpdatedAt,
        product.Category?.ToResponse());
}
