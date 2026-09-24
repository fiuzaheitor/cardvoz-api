using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.StudySessions;

public class RecordReviewDto
{
    [Required(ErrorMessage = "O identificador do cartão é obrigatório.")]
    public int CardId { get; set; }

    [Required(ErrorMessage = "O status de acerto/erro é obrigatório.")]
    public bool WasCorrect { get; set; }

    /// <summary>
    /// Indica se o estudante utilizou comando/resposta por voz.
    /// </summary>
    public bool VoiceUsed { get; set; } = false;
}
