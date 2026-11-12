using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Digite o seu e-mail.")]
    [EmailAddress(ErrorMessage = "Digite um e-mail válido, como nome@exemplo.com.")]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required(ErrorMessage = "O código do link é obrigatório.")]
    [MaxLength(100)]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Digite a nova senha.")]
    [MinLength(8, ErrorMessage = "A senha precisa ter pelo menos 8 caracteres.")]
    [MaxLength(128, ErrorMessage = "A senha não pode ter mais de 128 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
