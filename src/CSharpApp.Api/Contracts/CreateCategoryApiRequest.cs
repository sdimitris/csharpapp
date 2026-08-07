namespace CSharpApp.Api.Contracts;

public sealed record CreateCategoryApiRequest(string Name, string Image)
{
    /// <summary>
    /// Simple guard-clause validation. Returns the first violation found, or <c>null</c>
    /// when the request is valid.
    /// </summary>
    public Error? Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 100)
        {
            return Error.Validation("Category.Name", "Name is required and must be at most 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(Image))
        {
            return Error.Validation("Category.Image", "Image is required.");
        }

        return null;
    }
}
