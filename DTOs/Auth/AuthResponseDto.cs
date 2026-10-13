namespace cardvoz_api.DTOs.Auth;

/// <summary>
/// Resposta de cadastro, login e renovação. O token de acesso vai no cabeçalho
/// "Authorization: Bearer ..." das próximas requisições; o de renovação só é
/// enviado para /api/auth/refresh e /api/auth/logout.
/// </summary>
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public UserResponseDto User { get; set; } = new();
}
