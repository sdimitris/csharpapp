namespace CSharpApp.Core.Dtos.Auth;

/// <summary>
/// Credentials submitted to the third-party authentication endpoint.
/// </summary>
public sealed class AuthLoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
