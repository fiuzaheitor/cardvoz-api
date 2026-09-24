namespace cardvoz_api.DTOs.StudySessions;

public class StudyReviewResponseDto
{
    public int Id { get; set; }
    public int StudySessionId { get; set; }
    public int CardId { get; set; }
    public bool WasCorrect { get; set; }
    public bool VoiceUsed { get; set; }
    public DateTime AnsweredAt { get; set; }
}
