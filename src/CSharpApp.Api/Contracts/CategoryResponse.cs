namespace CSharpApp.Api.Contracts;

/// <summary>
/// The API's own public contract for a category — decoupled from the third-party
/// provider's <see cref="CSharpApp.Core.Dtos.CategoryFakePlatziDto"/> shape, so a change in the
/// provider's response never silently changes what this API returns to its own clients.
/// </summary>
public sealed record CategoryResponse(
    int Id,
    string Name,
    string Image,
    DateTime? CreationAt,
    DateTime? UpdatedAt);
