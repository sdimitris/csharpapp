namespace CSharpApp.Infrastructure.Http.Dtos;

/// <summary>
/// Shape of the "create category" payload expected by the third-party Platzi Fake Store API.
/// Used only for HTTP serialization at the Infrastructure boundary; built from this app's own
/// <see cref="CSharpApp.Core.Dtos.Requests.CreateCategoryRequest"/>.
/// </summary>
public sealed class CreateCategoryFakePlatziRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;
}
