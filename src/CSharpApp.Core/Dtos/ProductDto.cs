namespace CSharpApp.Core.Dtos;

/// <summary>
/// This API's own internal representation of a product, decoupled from the third-party
/// Platzi Fake Store API's <see cref="ProductFakePlatziDto"/> shape. Used across the
/// Application/Core layers (services, mappings) so a change in the provider's response
/// never silently changes internal behavior — only the Infrastructure layer knows about
/// <see cref="ProductFakePlatziDto"/>.
/// </summary>
public sealed class ProductDto
{
    public int? Id { get; set; }

    public string? Title { get; set; }

    public int? Price { get; set; }

    public string? Description { get; set; }

    public List<string> Images { get; set; } = [];

    public DateTime? CreationAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public CategoryDto? Category { get; set; }
}
