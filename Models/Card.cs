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
    /// Revisão espaçada (caixa de Leitner): nível de 0 a 6. "Errei" volta a 0; cada
    /// "Acertei" sobe um nível e afasta a próxima revisão (ver ReviewIntervals).
    /// </summary>
    public short ReviewLevel { get; set; } = 0;

    /// <summary>Quando a ficha volta para "Revisar agora". Nulo = nunca estudada.</summary>
    public DateTime? NextReviewAt { get; set; }

    public const short MaxReviewLevel = 6;

    /// <summary>
    /// Espera até a próxima revisão em cada nível: na hora, 1 dia, 3 dias, 1 semana,
    /// 2 semanas, 1 mês e 3 meses. O site usa a mesma tabela (src/lib/studyStats.ts).
    /// </summary>
    public static readonly TimeSpan[] ReviewIntervals =
    {
        TimeSpan.Zero, TimeSpan.FromDays(1), TimeSpan.FromDays(3), TimeSpan.FromDays(7),
        TimeSpan.FromDays(14), TimeSpan.FromDays(30), TimeSpan.FromDays(90)
    };

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
            ReviewLevel = wasCorrect ? (short)Math.Min(ReviewLevel + 1, MaxReviewLevel) : (short)0;
            NextReviewAt = answeredAt + ReviewIntervals[ReviewLevel];
        }
    }

    /// <summary>
    /// Acertos e erros que a ficha já tinha no aparelho (ao levar para a conta um
    /// baralho estudado sem login). A última resposta só vale com data e resultado juntos.
    /// </summary>
    public void SetHistory(
        int correctCount, int incorrectCount, DateTime? lastAnsweredAt, bool? lastWasCorrect,
        int? reviewLevel = null, DateTime? nextReviewAt = null)
    {
        CorrectCount = Math.Max(0, correctCount);
        IncorrectCount = Math.Max(0, incorrectCount);
        var complete = lastAnsweredAt.HasValue && lastWasCorrect.HasValue;
        LastAnsweredAt = complete ? lastAnsweredAt!.Value.ToUniversalTime() : null;
        LastWasCorrect = complete ? lastWasCorrect : null;
        if (!complete)
        {
            ReviewLevel = 0;
            NextReviewAt = null;
            return;
        }

        // Nível do aparelho; sem ele, o que a última resposta permite deduzir
        var level = reviewLevel ?? (lastWasCorrect!.Value ? 1 : 0);
        ReviewLevel = (short)Math.Clamp(level, 0, MaxReviewLevel);
        if (!lastWasCorrect!.Value) ReviewLevel = 0;
        NextReviewAt = nextReviewAt?.ToUniversalTime() ?? LastAnsweredAt + ReviewIntervals[ReviewLevel];
    }

    [JsonIgnore]
    public Deck? Deck { get; set; }
}
