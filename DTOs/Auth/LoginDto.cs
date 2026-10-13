using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [MaxLength(254, ErrorMessage = "O e-mail não pode ter mais de 254 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MaxLength(128, ErrorMessage = "A senha não pode ter mais de 128 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
