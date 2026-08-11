namespace CSharpApp.Core.Dtos.Requests;

/// <summary>
/// This API's own internal payload to create a new product, decoupled from the
/// third-party catalog service's wire shape (<c>CreateProductFakePlatziRequest</c> in
/// Infrastructure).
/// </summary>
public sealed class CreateProductRequest
{
    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public List<string> Images { get; set; } = [];
}
