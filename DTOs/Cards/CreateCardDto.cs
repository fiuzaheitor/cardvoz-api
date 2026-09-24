using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Cards;

public class CreateCardDto
{
    public int DeckId { get; set; }

    [Required(ErrorMessage = "A pergunta é obrigatória.")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "A resposta é obrigatória.")]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "As notas de voz não podem exceder 500 caracteres.")]
    public string? VoiceNotes { get; set; }

    public int DisplayOrder { get; set; } = 0;
}
