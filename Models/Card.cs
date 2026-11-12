using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace cardvoz_api.Models;

/// <summary>
/// Representa uma ficha individual de estudo (flashcard).
/// </summary>
public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeckId { get; set; }

    [Required]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Notas de voz, pronúncia alternativa ou palavras-chave esperadas para reconhecimento de voz.
    /// </summary>
    [MaxLength(500)]
    public string? VoiceNotes { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Quantas vezes a ficha foi respondida com "Acertei". Resumo de "StudyReviews",
    /// atualizado a cada resposta registrada.
    /// </summary>
    public int CorrectCount { get; set; } = 0;

    /// <summary>Quantas vezes a ficha foi respondida com "Errei".</summary>
    public int IncorrectCount { get; set; } = 0;

    /// <summary>Quando a ficha foi respondida pela última vez. Nulo = nunca estudada.</summary>
    public DateTime? LastAnsweredAt { get; set; }

    /// <summary>
    /// Resultado da resposta mais recente. Decide se a ficha entra em
    /// "Estudar só as que errei" no site. Nulo = nunca estudada.
    /// </summary>
    public bool? LastWasCorrect { get; set; }

    /// <summary>
    /// Soma uma resposta no resumo da ficha. A "última resposta" só muda se esta
    /// for mais nova: uma sessão feita sem internet pode chegar depois de outra mais recente.
    /// </summary>
    public void RecordAnswer(bool wasCorrect, DateTime answeredAt)
    {
        if (wasCorrect)
            CorrectCount++;
        else
            IncorrectCount++;

        if (!LastAnsweredAt.HasValue || answeredAt >= LastAnsweredAt.Value)
        {
            LastAnsweredAt = answeredAt;
            LastWasCorrect = wasCorrect;
        }
    }

    /// <summary>
    /// Acertos e erros que a ficha já tinha no aparelho (ao levar para a conta um
    /// baralho estudado sem login). A última resposta só vale com data e resultado juntos.
    /// </summary>
    public void SetHistory(int correctCount, int incorrectCount, DateTime? lastAnsweredAt, bool? lastWasCorrect)
    {
        CorrectCount = Math.Max(0, correctCount);
        IncorrectCount = Math.Max(0, incorrectCount);
        var complete = lastAnsweredAt.HasValue && lastWasCorrect.HasValue;
        LastAnsweredAt = complete ? lastAnsweredAt!.Value.ToUniversalTime() : null;
        LastWasCorrect = complete ? lastWasCorrect : null;
    }

    [JsonIgnore]
    public Deck? Deck { get; set; }
}
