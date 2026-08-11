namespace CSharpApp.Infrastructure.Http.Dtos;

/// <summary>
/// Shape of a category exactly as returned by the third-party Platzi Fake Store API. Used
/// only for HTTP (de)serialization at the Infrastructure boundary; the rest of the app works
/// with <see cref="CSharpApp.Core.Dtos.CategoryDto"/> instead.
/// </summary>
public sealed class CategoryFakePlatziDto
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("creationAt")]
    public DateTime? CreationAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}