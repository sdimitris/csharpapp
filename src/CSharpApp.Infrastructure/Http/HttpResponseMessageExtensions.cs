namespace CSharpApp.Infrastructure.Http;

/// <summary>
/// Maps raw <see cref="HttpResponseMessage"/> instances returned by the third-party service
/// into <see cref="Result{TValue}"/>, translating HTTP status codes into domain-friendly errors.
/// </summary>
internal static class HttpResponseMessageExtensions
{
    public static async Task<Result<TValue>> ToResultAsync<TValue>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<TValue>(cancellationToken: cancellationToken);

            return value is null
                ? Result.Failure<TValue>(Error.Unexpected("ThirdPartyService.EmptyResponse", "The third-party service returned an empty payload."))
                : Result.Success(value);
        }

        return Result.Failure<TValue>(await response.BuildErrorAsync(cancellationToken));
    }

    public static async Task<Error> BuildErrorAsync(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = string.IsNullOrWhiteSpace(body)
            ? response.ReasonPhrase ?? "The third-party service returned an error."
            : body;

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => Error.NotFound("ThirdPartyService.NotFound", message),
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity
                => Error.Validation("ThirdPartyService.BadRequest", message),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                => Error.Failure("ThirdPartyService.Unauthorized", message),
            HttpStatusCode.Conflict => Error.Conflict("ThirdPartyService.Conflict", message),
            _ => Error.Failure("ThirdPartyService.Error", message)
        };
    }
}
