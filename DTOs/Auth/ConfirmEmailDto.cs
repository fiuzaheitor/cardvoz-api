using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Auth;

public class ConfirmEmailDto
{
    [Required(ErrorMessage = "O código do link é obrigatório.")]
    [MaxLength(100)]
    public string Token { get; set; } = string.Empty;
}
