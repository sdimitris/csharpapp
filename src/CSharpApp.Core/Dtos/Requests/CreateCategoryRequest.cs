namespace CSharpApp.Core.Dtos.Requests;

/// <summary>
/// This API's own internal payload to create a new category, decoupled from the
/// third-party catalog service's wire shape (<c>CreateCategoryFakePlatziRequest</c> in
/// Infrastructure).
/// </summary>
public sealed class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;
}
