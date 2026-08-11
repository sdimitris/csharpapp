namespace CSharpApp.Infrastructure.Http.Dtos;

/// <summary>
/// Shape of a product exactly as returned by the third-party Platzi Fake Store API
/// (https://api.escuelajs.co). Used only for HTTP (de)serialization at the Infrastructure
/// boundary; the rest of the app works with <see cref="CSharpApp.Core.Dtos.ProductDto"/>
/// instead so a change in the provider's response never silently ripples through the codebase.
/// </summary>
public sealed class ProductFakePlatziDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("price")]
    public int? Price { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = [];

    [JsonPropertyName("creationAt")]
    public DateTime? CreationAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("category")]
    public CategoryFakePlatziDto? Category { get; set; }
}