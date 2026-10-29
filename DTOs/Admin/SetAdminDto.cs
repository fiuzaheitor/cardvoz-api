using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Admin;

public class SetAdminDto
{
    /// <summary>Obrigatório: um corpo vazio ou com o campo errado não pode tirar o admin.</summary>
    [Required(ErrorMessage = "Informe isAdmin (true ou false).")]
    public bool? IsAdmin { get; set; }
}
