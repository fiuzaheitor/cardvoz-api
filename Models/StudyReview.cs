using System.Text.Json.Serialization;

namespace cardvoz_api.Models;

/// <summary>
/// Registra a resposta de um flashcard dentro de uma sessão de estudo.
/// </summary>
public class StudyReview
{
    public int Id { get; set; }

    public int StudySessionId { get; set; }

    public int CardId { get; set; }

    public bool WasCorrect { get; set; }

    /// <summary>
    /// Indica se o comando/resposta utilizou voz ou reconhecimento auditivo.
    /// </summary>
    public bool VoiceUsed { get; set; }

    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public StudySession? StudySession { get; set; }

    [JsonIgnore]
    public Card? Card { get; set; }
}
