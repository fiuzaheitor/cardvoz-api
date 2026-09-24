using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Cards;

public class UpdateCardDto
{
    public string? Question { get; set; }

    public string? Answer { get; set; }

    [MaxLength(500, ErrorMessage = "As notas de voz não podem exceder 500 caracteres.")]
    public string? VoiceNotes { get; set; }

    public int? DisplayOrder { get; set; }
}
