using cardvoz_api.Data;
using cardvoz_api.DTOs.StudySessions;
using cardvoz_api.Models;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Services;

public class StudyService : IStudyService
{
    private const int MaxHistoryLimit = 100;

    private readonly CardVozDbContext _context;
    private readonly ILogger<StudyService> _logger;

    public StudyService(CardVozDbContext context, ILogger<StudyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StudySessionResponseDto> StartSessionAsync(Guid userId, StartSessionDto dto)
    {
        Deck? deck = null;
        var ownDecks = _context.Decks.Include(d => d.Cards).Where(d => d.UserId == userId);

        if (dto.DeckId.HasValue)
        {
            deck = await ownDecks.FirstOrDefaultAsync(d => d.Id == dto.DeckId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(dto.DeckSlug))
        {
            var slug = dto.DeckSlug.Trim().ToLower();
            deck = await ownDecks.FirstOrDefaultAsync(d => d.Slug == slug);
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

    public async Task<StudyReviewResponseDto?> RecordReviewAsync(Guid userId, Guid sessionId, RecordReviewDto dto)
    {
        var session = await _context.StudySessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Deck!.UserId == userId);
        if (session == null)
            return null;

        if (session.CompletedAt.HasValue)
        {
            throw new ArgumentException("Esta sessão de estudo já foi finalizada.");
        }

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

    public async Task<StudySessionResponseDto?> FinishSessionAsync(Guid userId, Guid sessionId)
    {
        var session = await _context.StudySessions
            .Include(s => s.Deck)
            .Include(s => s.Reviews)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Deck!.UserId == userId);

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

    public async Task<StudySessionResponseDto?> GetSessionByIdAsync(Guid userId, Guid sessionId)
    {
        var session = await _context.StudySessions
            .AsNoTracking()
            .Include(s => s.Deck)
            .Include(s => s.Reviews)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Deck!.UserId == userId);

        if (session == null)
            return null;

        return MapToSessionDto(session);
    }

    public async Task<IEnumerable<StudySessionResponseDto>> GetHistoryAsync(Guid userId, Guid? deckId = null, int limit = 20)
    {
        // Limite máximo para ninguém pedir o histórico inteiro de uma vez
        limit = Math.Clamp(limit, 1, MaxHistoryLimit);

        var query = _context.StudySessions
            .AsNoTracking()
            .Include(s => s.Deck)
            .Include(s => s.Reviews)
            .Where(s => s.Deck!.UserId == userId);

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
