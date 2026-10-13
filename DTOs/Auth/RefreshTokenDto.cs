using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Auth;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "O token de renovação é obrigatório.")]
    [MaxLength(200, ErrorMessage = "Token de renovação inválido.")]
    public string RefreshToken { get; set; } = string.Empty;
}
