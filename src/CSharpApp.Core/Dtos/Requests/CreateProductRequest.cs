namespace CSharpApp.Core.Dtos.Requests;

/// <summary>
/// Payload required to create a new product against the third-party catalog service.
/// </summary>
public sealed class CreateProductRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = [];
}
