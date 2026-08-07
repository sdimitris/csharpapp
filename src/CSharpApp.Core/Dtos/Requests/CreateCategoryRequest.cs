namespace CSharpApp.Core.Dtos.Requests;

/// <summary>
/// Payload required to create a new category against the third-party catalog service.
/// </summary>
public sealed class CreateCategoryRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;
}
