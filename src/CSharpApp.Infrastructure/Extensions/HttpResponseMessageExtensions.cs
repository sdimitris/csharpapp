namespace CSharpApp.Infrastructure.Extensions;

/// <summary>
/// Maps raw <see cref="HttpResponseMessage"/> instances returned by the upstream service into
/// <see cref="Result{TValue}"/>. Responses are always generic/provider-agnostic (callers should
/// never learn which third-party API is behind this app); the raw upstream details are logged
/// via the caller's own <see cref="ILogger"/> for diagnostics only.
/// </summary>
internal static class HttpResponseMessageExtensions
{
    public static async Task<Result<TValue>> ToResultAsync<TValue>(
        this HttpResponseMessage response,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<TValue>(cancellationToken: cancellationToken);

            return value is null
                ? Result.Failure<TValue>(Error.Unexpected("Response.Empty", "The service returned an empty response."))
                : Result.Success(value);
        }

        return Result.Failure<TValue>(await response.BuildErrorAsync(logger, cancellationToken));
    }

    public static async Task<Error> BuildErrorAsync(this HttpResponseMessage response, ILogger logger, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        LogUpstreamFailure(logger, response.StatusCode, body);

        if (IsEntityNotFound(body))
        {
            return Error.NotFound("Resource.NotFound", "The requested resource was not found.");
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => Error.NotFound("Resource.NotFound", "The requested resource was not found."),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                => Error.Failure("Request.Unauthorized", "You are not authorized to perform this action."),
            HttpStatusCode.Conflict => Error.Conflict("Resource.Conflict", "The resource already exists or conflicts with an existing one."),
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity
                => Error.Validation("Request.Invalid", "The request could not be processed."),
            _ => Error.Failure("Request.Invalid", "An error occurred while processing the request.")
        };
    }

    /// <summary>
    /// Detects the upstream's <c>EntityNotFoundError</c> shape, which it returns with a 400
    /// status instead of 404 for missing resources.
    /// </summary>
    private static bool IsEntityNotFound(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("name", out var name)
                && string.Equals(name.GetString(), "EntityNotFoundError", StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Logs the raw upstream failure (status, name/message/code when present) for developer
    /// diagnostics. Never surfaced in API responses.
    /// </summary>
    private static void LogUpstreamFailure(ILogger logger, HttpStatusCode statusCode, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            logger.LogError("Upstream request failed with status {StatusCode} and an empty body.", (int)statusCode);
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var name = root.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
            var message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() : null;
            var code = root.TryGetProperty("code", out var codeElement) ? codeElement.GetString() : null;

            logger.LogError(
                "Upstream request failed with status {StatusCode}. Name: {Name}, Message: {Message}, Code: {Code}",
                (int)statusCode, name, message, code);
        }
        catch (JsonException)
        {
            logger.LogError("Upstream request failed with status {StatusCode}. Body: {Body}", (int)statusCode, body);
        }
    }
}
