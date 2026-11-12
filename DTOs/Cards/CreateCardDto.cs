using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Cards;

public class CreateCardDto
{
    public Guid DeckId { get; set; }

    [Required(ErrorMessage = "A pergunta é obrigatória.")]
    [MaxLength(5000, ErrorMessage = "A pergunta não pode ter mais de 5000 caracteres.")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "A resposta é obrigatória.")]
    [MaxLength(5000, ErrorMessage = "A resposta não pode ter mais de 5000 caracteres.")]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "As notas de voz não podem exceder 500 caracteres.")]
    public string? VoiceNotes { get; set; }

    public int DisplayOrder { get; set; } = 0;

    // Acertos e erros que a ficha já tinha no aparelho. O site os envia quando a
    // pessoa leva para a conta um baralho que estudou sem login, para o histórico
    // não recomeçar do zero. Sem eles, a ficha nasce sem estudo.

    [Range(0, 100000, ErrorMessage = "A quantidade de acertos precisa estar entre 0 e 100000.")]
    public int CorrectCount { get; set; } = 0;

    [Range(0, 100000, ErrorMessage = "A quantidade de erros precisa estar entre 0 e 100000.")]
    public int IncorrectCount { get; set; } = 0;

    public DateTime? LastAnsweredAt { get; set; }

    public bool? LastWasCorrect { get; set; }
}
