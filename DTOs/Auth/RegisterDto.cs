using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [MaxLength(254, ErrorMessage = "O e-mail não pode ter mais de 254 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha precisa ter pelo menos 8 caracteres.")]
    [MaxLength(128, ErrorMessage = "A senha não pode ter mais de 128 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
