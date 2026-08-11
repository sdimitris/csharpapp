namespace CSharpApp.Api.Extensions;

/// <summary>
/// Adapts application <see cref="Result{TValue}"/>/<see cref="Error"/> values into ASP.NET
/// Core minimal-API <see cref="IResult"/> responses. This is presentation/HTTP-transport
/// glue, not a domain mapping, so it lives separately from <c>Mappings</c>.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Projects a successful <see cref="Result{TValue}"/> into a different value (e.g. an
    /// internal model into this API's own public response contract), leaving a failed
    /// result's error untouched.
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> map) =>
        result.IsSuccess ? Result.Success(map(result.Value)) : Result.Failure<TOut>(result.Error);

    /// <summary>
    /// Converts an application <see cref="Result{TValue}"/> into the appropriate minimal-API
    /// <see cref="IResult"/>, mapping <see cref="ErrorType"/> to the matching HTTP status code.
    /// </summary>
    public static IResult ToApiResult<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return result.Error.Type switch
        {
            ErrorType.Validation => Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [result.Error.Code] = [result.Error.Message]
            }),
            
            ErrorType.NotFound => Results.Problem(result.Error.Message, statusCode: StatusCodes.Status404NotFound, title: result.Error.Code),
            ErrorType.Conflict => Results.Problem(result.Error.Message, statusCode: StatusCodes.Status409Conflict, title: result.Error.Code),
            _ => Results.Problem(result.Error.Message, statusCode: StatusCodes.Status502BadGateway, title: result.Error.Code)
        };
    }

    /// <summary>
    /// Converts a successful create <see cref="Result{TValue}"/> into a 201 Created response.
    /// </summary>
    public static IResult ToCreatedApiResult<TValue>(this Result<TValue> result, Func<TValue, string> locationFactory)
    {
        if (result.IsSuccess)
        {
            return Results.Created(locationFactory(result.Value), result.Value);
        }

        return ((Result<TValue>)Result.Failure<TValue>(result.Error)).ToApiResult();
    }

    /// <summary>
    /// Converts a request-level validation <see cref="Error"/> (produced before ever calling
    /// the third-party service) into a 400 response with the same shape as downstream
    /// validation failures.
    /// </summary>
    public static IResult ToValidationApiResult(this Error error) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [error.Code] = [error.Message]
        });
}
