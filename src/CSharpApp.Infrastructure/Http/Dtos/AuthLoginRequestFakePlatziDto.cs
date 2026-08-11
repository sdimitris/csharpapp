namespace CSharpApp.Infrastructure.Http.Dtos;

/// <summary>
/// Credentials submitted to the third-party authentication endpoint. Used only for HTTP
/// serialization at the Infrastructure boundary — never surfaced outside <see cref="Authenticator"/>.
/// </summary>
public sealed class AuthLoginRequestFakePlatziDto
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
