namespace CSharpApp.Api.Contracts;

public sealed record CreateProductApiRequest(
    string Title,
    decimal Price,
    string Description,
    int CategoryId,
    List<string> Images)
{
    /// <summary>
    /// Simple guard-clause validation. Returns the first violation found, or <c>null</c>
    /// when the request is valid.
    /// </summary>
    public Error? Validate()
    {
        if (string.IsNullOrWhiteSpace(Title) || Title.Length > 200)
        {
            return Error.Validation("Product.Title", "Title is required and must be at most 200 characters.");
        }

        if (Price <= 0)
        {
            return Error.Validation("Product.Price", "Price must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            return Error.Validation("Product.Description", "Description is required.");
        }

        if (CategoryId <= 0)
        {
            return Error.Validation("Product.CategoryId", "CategoryId must be greater than zero.");
        }

        if (Images is null || Images.Count == 0 || Images.Any(string.IsNullOrWhiteSpace))
        {
            return Error.Validation("Product.Images", "At least one non-empty image URL is required.");
        }

        return null;
    }
}
