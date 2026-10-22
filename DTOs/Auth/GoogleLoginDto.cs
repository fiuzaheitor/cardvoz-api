using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Auth;

/// <summary>
/// Dados que o site recebe na volta do Google. A API troca o código pelo
/// id_token usando o segredo do cliente, que nunca vai para o navegador.
/// </summary>
public class GoogleLoginDto
{
    [Required(ErrorMessage = "O código do Google é obrigatório.")]
    [MaxLength(2048, ErrorMessage = "Código do Google inválido.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>Verificador PKCE gerado pelo site antes de ir para o Google.</summary>
    [Required(ErrorMessage = "O verificador do login é obrigatório.")]
    [MinLength(43, ErrorMessage = "Verificador do login inválido.")]
    [MaxLength(128, ErrorMessage = "Verificador do login inválido.")]
    public string CodeVerifier { get; set; } = string.Empty;

    /// <summary>Endereço usado na ida para o Google (ex.: https://cardvoz.onrender.com/entrar/).</summary>
    [Required(ErrorMessage = "O endereço de retorno é obrigatório.")]
    [MaxLength(500, ErrorMessage = "Endereço de retorno inválido.")]
    public string RedirectUri { get; set; } = string.Empty;
}
