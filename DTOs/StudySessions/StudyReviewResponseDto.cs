namespace cardvoz_api.DTOs.StudySessions;

public class StudyReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid StudySessionId { get; set; }
    public Guid CardId { get; set; }
    public bool WasCorrect { get; set; }
    public bool VoiceUsed { get; set; }
    public DateTime AnsweredAt { get; set; }
}
