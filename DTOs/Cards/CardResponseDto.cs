namespace cardvoz_api.DTOs.Cards;

public class CardResponseDto
{
    public Guid Id { get; set; }
    public Guid DeckId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? VoiceNotes { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Quantas vezes a ficha foi respondida com "Acertei".</summary>
    public int CorrectCount { get; set; }

    /// <summary>Quantas vezes a ficha foi respondida com "Errei".</summary>
    public int IncorrectCount { get; set; }

    /// <summary>Quando a ficha foi respondida pela última vez (nulo = nunca).</summary>
    public DateTime? LastAnsweredAt { get; set; }

    /// <summary>Resultado da última resposta (nulo = nunca estudada).</summary>
    public bool? LastWasCorrect { get; set; }

    /// <summary>
    /// Revisão espaçada: facilidade em milésimos (2500 = 2,5). O intervalo da ficha
    /// são os dias entre LastAnsweredAt e NextReviewAt.
    /// </summary>
    public int ReviewEase { get; set; }

    /// <summary>Quando a ficha volta para "Revisar agora" (nulo = nunca estudada).</summary>
    public DateTime? NextReviewAt { get; set; }
}
