namespace CSharpApp.Infrastructure.Http.Dtos;

/// <summary>
/// Shape of the "create product" payload expected by the third-party Platzi Fake Store API.
/// Used only for HTTP serialization at the Infrastructure boundary; built from this app's own
/// <see cref="CSharpApp.Core.Dtos.Requests.CreateProductRequest"/>.
/// </summary>
public sealed class CreateProductFakePlatziRequest
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
