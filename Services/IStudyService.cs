using cardvoz_api.DTOs.StudySessions;

namespace cardvoz_api.Services;

/// <summary>
/// Sessões de estudo pertencem ao dono do baralho estudado.
/// </summary>
public interface IStudyService
{
    Task<StudySessionResponseDto> StartSessionAsync(Guid userId, StartSessionDto dto);
    Task<StudyReviewResponseDto?> RecordReviewAsync(Guid userId, Guid sessionId, RecordReviewDto dto);
    Task<StudySessionResponseDto?> FinishSessionAsync(Guid userId, Guid sessionId);
    Task<StudySessionResponseDto?> GetSessionByIdAsync(Guid userId, Guid sessionId);
    Task<IEnumerable<StudySessionResponseDto>> GetHistoryAsync(Guid userId, Guid? deckId = null, int limit = 20);
}
