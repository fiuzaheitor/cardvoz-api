using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Account;

/// <summary>
/// Confirmação para excluir a conta. A pessoa digita o próprio e-mail (todas as
/// contas) e a senha (contas com senha): um token roubado sozinho não apaga a conta.
/// </summary>
public class DeleteAccountDto
{
    [Required(ErrorMessage = "Digite o seu e-mail para confirmar.")]
    [MaxLength(254)]
    public string ConfirmEmail { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Password { get; set; }
}
