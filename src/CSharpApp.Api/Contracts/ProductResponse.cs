namespace CSharpApp.Api.Contracts;

/// <summary>
/// The API's own public contract for a product — decoupled from the third-party
/// provider's <see cref="CSharpApp.Core.Dtos.Product"/> shape, so a change in the
/// provider's response never silently changes what this API returns to its own clients.
/// </summary>
public sealed record ProductResponse(
    int Id,
    string Title,
    decimal Price,
    string Description,
    IReadOnlyList<string> Images,
    DateTime? CreationAt,
    DateTime? UpdatedAt,
    CategoryResponse? Category);
