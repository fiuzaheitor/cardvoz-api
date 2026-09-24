using cardvoz_api.Data;
using cardvoz_api.DTOs.StudySessions;
using cardvoz_api.Models;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Services;

public class StudyService : IStudyService
{
    private readonly CardVozDbContext _context;
    private readonly ILogger<StudyService> _logger;

    public StudyService(CardVozDbContext context, ILogger<StudyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StudySessionResponseDto> StartSessionAsync(StartSessionDto dto)
    {
        Deck? deck = null;

        if (dto.DeckId.HasValue)
        {
            deck = await _context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.Id == dto.DeckId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(dto.DeckSlug))
        {
            deck = await _context.Decks
                .Include(d => d.Cards)
                .FirstOrDefaultAsync(d => d.Slug == dto.DeckSlug.Trim().ToLower());
        }

        if (deck == null)
        {
            throw new KeyNotFoundException("Baralho não encontrado para iniciar a sessão.");
        }

        var session = new StudySession
        {
            DeckId = deck.Id,
            StartedAt = DateTime.UtcNow,
            TotalCards = deck.Cards.Count,
            CorrectCount = 0,
            IncorrectCount = 0
        };

        _context.StudySessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Sessão iniciada: ID {SessionId} para o Baralho {DeckId}", session.Id, deck.Id);

        return new StudySessionResponseDto
        {
            Id = session.Id,
            DeckId = session.DeckId,
            DeckTitle = deck.Title,
            DeckSlug = deck.Slug,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            TotalCards = session.TotalCards,
            CorrectCount = session.CorrectCount,
            IncorrectCount = session.IncorrectCount,
            Reviews = new List<StudyReviewResponseDto>()
        };
    }

    public async Task<StudyReviewResponseDto?> RecordReviewAsync(int sessionId, RecordReviewDto dto)
    {
        var session = await _context.StudySessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null)
            return null;

        var card = await _context.Cards.FirstOrDefaultAsync(c => c.Id == dto.CardId && c.DeckId == session.DeckId);
        if (card == null)
        {
            throw new ArgumentException($"O cartão com ID {dto.CardId} não pertence a este baralho.");
        }

        var review = new StudyReview
        {
            StudySessionId = sessionId,
            CardId = dto.CardId,
            WasCorrect = dto.WasCorrect,
            VoiceUsed = dto.VoiceUsed,
            AnsweredAt = DateTime.UtcNow
        };

        _context.StudyReviews.Add(review);

        if (dto.WasCorrect)
            session.CorrectCount++;
        else
            session.IncorrectCount++;

        await _context.SaveChangesAsync();

        return new StudyReviewResponseDto
        {
            Id = review.Id,
            StudySessionId = review.StudySessionId,
            CardId = review.CardId,
            WasCorrect = review.WasCorrect,
            VoiceUsed = review.VoiceUsed,
            AnsweredAt = review.AnsweredAt
        };
    }

    public async Task<StudySessionResponseDto?> FinishSessionAsync(int sessionId)
    {
        var session = await _context.StudySessions
            .Include(s => s.Deck)
            .Include(s => s.Reviews)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            return null;

        if (!session.CompletedAt.HasValue)
        {
            session.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Sessão finalizada: ID {SessionId}", session.Id);
        }

        return MapToSessionDto(session);
    }

    public async Task<StudySessionResponseDto?> GetSessionByIdAsync(int sessionId)
    {
        var session = await _context.StudySessions
            .AsNoTracking()
            .Include(s => s.Deck)
            .Include(s => s.Reviews)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            return null;

        return MapToSessionDto(session);
    }

    public async Task<IEnumerable<StudySessionResponseDto>> GetHistoryAsync(int? deckId = null, int limit = 20)
    {
        var query = _context.StudySessions
            .AsNoTracking()
            .Include(s => s.Deck)
            .Include(s => s.Reviews)
            .AsQueryable();

        if (deckId.HasValue)
        {
            query = query.Where(s => s.DeckId == deckId.Value);
        }

        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .Take(limit)
            .ToListAsync();

        return sessions.Select(MapToSessionDto);
    }

    private static StudySessionResponseDto MapToSessionDto(StudySession session)
    {
        return new StudySessionResponseDto
        {
            Id = session.Id,
            DeckId = session.DeckId,
            DeckTitle = session.Deck?.Title,
            DeckSlug = session.Deck?.Slug,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            TotalCards = session.TotalCards,
            CorrectCount = session.CorrectCount,
            IncorrectCount = session.IncorrectCount,
            Reviews = session.Reviews?.Select(r => new StudyReviewResponseDto
            {
                Id = r.Id,
                StudySessionId = r.StudySessionId,
                CardId = r.CardId,
                WasCorrect = r.WasCorrect,
                VoiceUsed = r.VoiceUsed,
                AnsweredAt = r.AnsweredAt
            }).OrderBy(r => r.AnsweredAt).ToList()
        };
    }
}
