using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Admin;

public class SharedCardDto
{
    [Required(ErrorMessage = "A pergunta é obrigatória.")]
    [MaxLength(5000, ErrorMessage = "A pergunta não pode ter mais de 5000 caracteres.")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "A resposta é obrigatória.")]
    [MaxLength(5000, ErrorMessage = "A resposta não pode ter mais de 5000 caracteres.")]
    public string Answer { get; set; } = string.Empty;
}
