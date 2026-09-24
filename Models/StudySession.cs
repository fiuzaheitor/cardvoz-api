using System.Text.Json.Serialization;

namespace cardvoz_api.Models;

/// <summary>
/// Representa uma sessão de estudo realizada pelo usuário em determinado baralho.
/// </summary>
public class StudySession
{
    public int Id { get; set; }

    public int DeckId { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public int TotalCards { get; set; } = 0;

    public int CorrectCount { get; set; } = 0;

    public int IncorrectCount { get; set; } = 0;

    [JsonIgnore]
    public Deck? Deck { get; set; }

    public ICollection<StudyReview> Reviews { get; set; } = new List<StudyReview>();
}
