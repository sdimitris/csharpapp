namespace CSharpApp.Core.Dtos;

/// <summary>
/// This API's own internal representation of a category, decoupled from the third-party
/// Platzi Fake Store API's <see cref="CategoryFakePlatziDto"/> shape.
/// </summary>
public sealed class CategoryDto
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? Image { get; set; }

    public DateTime? CreationAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
