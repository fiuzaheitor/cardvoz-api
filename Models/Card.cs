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

    /// <summary>Quando a ficha volta para "Revisar agora". Nulo = nunca estudada.</summary>
    public DateTime? NextReviewAt { get; set; }

    /// <summary>
    /// Revisão espaçada no estilo do Anki (algoritmo SM-2, com duas respostas: "Acertei"
    /// vale como "Bom" e "Errei" como "De novo"). Facilidade da ficha em milésimos
    /// (2500 = 2,5): por quanto o intervalo é multiplicado a cada "Acertei". Cai a cada
    /// erro de uma ficha em revisão.
    /// </summary>
    public short ReviewEase { get; set; } = DefaultReviewEase;

    // Os mesmos valores do site (src/lib/studyStats.ts): mude os dois juntos
    public const short DefaultReviewEase = 2500;
    public const short MinReviewEase = 1300;
    /// <summary>Quanto a facilidade cai quando a pessoa erra uma ficha que estava em revisão.</summary>
    public const short LapseEasePenalty = 200;
    /// <summary>Nenhuma ficha fica mais de 1 ano sem voltar.</summary>
    public const int MaxReviewIntervalDays = 365;

    private const long DayMs = 86_400_000;

    /// <summary>
    /// Intervalo em vigor, em dias: os dias entre a última resposta e a próxima revisão.
    /// 0 = ficha nova, ou errada da última vez. Não é guardado à parte para não divergir
    /// das datas (que a API anterior, com a caixa de Leitner, também gravava).
    /// </summary>
    public int ReviewIntervalDays
    {
        get
        {
            if (LastWasCorrect != true || !LastAnsweredAt.HasValue) return 0;
            if (!NextReviewAt.HasValue) return 1;
            // Quem acertou por último tem pelo menos 1 dia
            return (int)Math.Clamp(RoundDiv(Ms(NextReviewAt.Value - LastAnsweredAt.Value), DayMs), 1, MaxReviewIntervalDays);
        }
    }

    /// <summary>
    /// Soma uma resposta no resumo da ficha. A "última resposta" e a revisão só mudam
    /// se esta for mais nova: uma sessão feita sem internet pode chegar depois de outra mais recente.
    /// </summary>
    public void RecordAnswer(bool wasCorrect, DateTime answeredAt)
    {
        if (wasCorrect)
            CorrectCount++;
        else
            IncorrectCount++;

        if (LastAnsweredAt.HasValue && answeredAt < LastAnsweredAt.Value)
            return;

        var days = 0;
        if (wasCorrect)
        {
            days = NextIntervalDays(answeredAt);
        }
        else if (ReviewIntervalDays > 0)
        {
            // Só o erro de uma ficha que estava em revisão a deixa mais difícil;
            // errar de novo antes de acertar não baixa a facilidade outra vez
            ReviewEase = (short)Math.Max(MinReviewEase, ReviewEase - LapseEasePenalty);
        }

        LastAnsweredAt = answeredAt;
        LastWasCorrect = wasCorrect;
        // Errou: 0 dias, volta para "Revisar agora" na hora
        NextReviewAt = answeredAt.AddDays(days);
    }

    /// <summary>Intervalo depois de um "Acertei" dado em answeredAt. Só contas com inteiros, iguais às do site.</summary>
    private int NextIntervalDays(DateTime answeredAt)
    {
        var current = ReviewIntervalDays;
        // Ficha nova, ou errada da última vez: volta amanhã
        if (current <= 0)
            return 1;

        long next;
        if (NextReviewAt.HasValue && answeredAt >= NextReviewAt.Value)
        {
            // Na data ou atrasada: metade do atraso conta como intervalo cumprido,
            // e o intervalo sempre cresce pelo menos 1 dia
            var lateMs = Ms(answeredAt - NextReviewAt.Value);
            next = Math.Max(current + 1, RoundDiv((2 * current * DayMs + lateMs) * ReviewEase, 2000 * DayMs));
        }
        else
        {
            // Antes da data (ex.: "Estudar" com todas as fichas): conta só o tempo que
            // passou desde a última resposta, e o intervalo não encolhe. Estudar a
            // mesma ficha todo dia não a empurra para daqui a meses
            var elapsedMs = Ms(answeredAt - LastAnsweredAt!.Value);
            next = Math.Max(current, RoundDiv(elapsedMs * ReviewEase, 1000 * DayMs));
        }
        return (int)Math.Min(next, MaxReviewIntervalDays);
    }

    private static long Ms(TimeSpan span) => Math.Max(0, span.Ticks / TimeSpan.TicksPerMillisecond);

    /// <summary>Divisão de inteiros positivos arredondada para o mais próximo (meio para cima).</summary>
    private static long RoundDiv(long a, long b) => (a + b / 2) / b;

    /// <summary>
    /// Acertos, erros e revisão que a ficha já tinha no aparelho (ao levar para a conta um
    /// baralho estudado sem login). A última resposta só vale com data e resultado juntos.
    /// </summary>
    public void SetHistory(
        int correctCount, int incorrectCount, DateTime? lastAnsweredAt, bool? lastWasCorrect,
        int? reviewEase = null, DateTime? nextReviewAt = null)
    {
        CorrectCount = Math.Max(0, correctCount);
        IncorrectCount = Math.Max(0, incorrectCount);
        ReviewEase = (short)Math.Clamp(reviewEase ?? DefaultReviewEase, MinReviewEase, DefaultReviewEase);
        if (!lastAnsweredAt.HasValue || !lastWasCorrect.HasValue)
        {
            LastAnsweredAt = null;
            LastWasCorrect = null;
            NextReviewAt = null;
            return;
        }

        var answeredAt = lastAnsweredAt.Value.ToUniversalTime();
        var next = nextReviewAt?.ToUniversalTime();
        // Relógio do aparelho adiantado: uma "última resposta" no futuro faria os
        // estudos seguintes (com a hora certa) parecerem mais antigos, e a revisão
        // da ficha não andaria até passar essa data. Igual ao Clamp do /complete.
        // A próxima revisão recua junto, para o intervalo continuar o mesmo
        var now = DateTime.UtcNow;
        if (answeredAt > now)
        {
            next -= answeredAt - now;
            answeredAt = now;
        }
        LastAnsweredAt = answeredAt;
        LastWasCorrect = lastWasCorrect;
        if (!lastWasCorrect.Value)
        {
            // Errou por último: revisar na hora
            NextReviewAt = answeredAt;
            return;
        }

        // Acertou por último: a data do aparelho, entre 1 dia e 1 ano depois da última
        // resposta (o intervalo da ficha sai dessas duas datas); sem a data, 1 dia
        var first = answeredAt.AddDays(1);
        var last = answeredAt.AddDays(MaxReviewIntervalDays);
        NextReviewAt = !next.HasValue || next.Value < first ? first : next.Value > last ? last : next.Value;
    }

    [JsonIgnore]
    public Deck? Deck { get; set; }
}
