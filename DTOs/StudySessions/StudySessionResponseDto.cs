namespace cardvoz_api.DTOs.StudySessions;

public class StudySessionResponseDto
{
    public Guid Id { get; set; }
    public Guid DeckId { get; set; }
    public string? DeckTitle { get; set; }
    public string? DeckSlug { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalCards { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public double ScorePercentage => TotalCards > 0 ? Math.Round((double)CorrectCount / TotalCards * 100, 1) : 0;
    public bool IsCompleted => CompletedAt.HasValue;
    public IEnumerable<StudyReviewResponseDto>? Reviews { get; set; }
}
