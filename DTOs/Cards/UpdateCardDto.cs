using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Cards;

public class UpdateCardDto
{
    [MaxLength(5000, ErrorMessage = "A pergunta não pode ter mais de 5000 caracteres.")]
    public string? Question { get; set; }

    [MaxLength(5000, ErrorMessage = "A resposta não pode ter mais de 5000 caracteres.")]
    public string? Answer { get; set; }

    [MaxLength(500, ErrorMessage = "As notas de voz não podem exceder 500 caracteres.")]
    public string? VoiceNotes { get; set; }

    public int? DisplayOrder { get; set; }
}
