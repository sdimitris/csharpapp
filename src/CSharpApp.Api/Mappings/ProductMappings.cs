namespace CSharpApp.Api.Mappings;

/// <summary>
/// Maps the internal, provider-shaped <see cref="Product"/> model to this API's own
/// public <see cref="ProductResponse"/> contract.
/// </summary>
public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id ?? 0,
        product.Title ?? string.Empty,
        product.Price ?? 0,
        product.Description ?? string.Empty,
        product.Images,
        product.CreationAt,
        product.UpdatedAt,
        product.Category?.ToResponse());
}
