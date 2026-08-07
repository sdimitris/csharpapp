namespace CSharpApp.Core.Dtos.Auth;

/// <summary>
/// JWT token pair returned by the third-party authentication endpoint.
/// </summary>
public sealed class AuthLoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
