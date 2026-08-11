namespace CSharpApp.Core.Dtos;

/// <summary>
/// This API's own internal representation of the JWT token pair obtained from the
/// third-party authentication endpoint, decoupled from its wire shape
/// (<c>AuthLoginResponseFakePlatziDto</c> in Infrastructure).
/// </summary>
public sealed class AuthTokenDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}
