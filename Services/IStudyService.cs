using cardvoz_api.DTOs.StudySessions;

namespace cardvoz_api.Services;

public interface IStudyService
{
    Task<StudySessionResponseDto> StartSessionAsync(StartSessionDto dto);
    Task<StudyReviewResponseDto?> RecordReviewAsync(int sessionId, RecordReviewDto dto);
    Task<StudySessionResponseDto?> FinishSessionAsync(int sessionId);
    Task<StudySessionResponseDto?> GetSessionByIdAsync(int sessionId);
    Task<IEnumerable<StudySessionResponseDto>> GetHistoryAsync(int? deckId = null, int limit = 20);
}
