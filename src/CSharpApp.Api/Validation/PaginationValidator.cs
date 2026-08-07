namespace CSharpApp.Api.Validation;

/// <summary>
/// Guard-clause validation for the <c>offset</c>/<c>limit</c> pagination query parameters
/// shared by the products and categories list endpoints.
/// </summary>
public static class PaginationValidator
{
    private const int MaxLimit = 100;

    public static Error? Validate(int? offset, int? limit)
    {
        if (offset is < 0)
        {
            return Error.Validation("Pagination.Offset", "Offset must be greater than or equal to zero.");
        }

        if (limit is <= 0)
        {
            return Error.Validation("Pagination.Limit", "Limit must be greater than zero.");
        }

        if (limit > MaxLimit)
        {
            return Error.Validation("Pagination.Limit", $"Limit must be at most {MaxLimit}.");
        }

        return null;
    }
}
