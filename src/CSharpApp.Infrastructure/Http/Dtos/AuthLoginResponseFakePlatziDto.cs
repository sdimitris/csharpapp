namespace CSharpApp.Infrastructure.Http.Dtos;

/// <summary>
/// JWT token pair returned by the third-party authentication endpoint. Used only for HTTP
/// deserialization at the Infrastructure boundary; mapped to <see cref="CSharpApp.Core.Dtos.AuthTokenDto"/>
/// before being returned to the rest of the app.
/// </summary>
public sealed class AuthLoginResponseFakePlatziDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
