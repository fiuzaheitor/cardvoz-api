using cardvoz_api.Data;
using cardvoz_api.DTOs.Cards;
using cardvoz_api.Models;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Services;

public class CardService : ICardService
{
    private readonly CardVozDbContext _context;
    private readonly ILogger<CardService> _logger;

    public CardService(CardVozDbContext context, ILogger<CardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CardResponseDto>> GetByDeckIdAsync(int deckId)
    {
        return await _context.Cards
            .AsNoTracking()
            .Where(c => c.DeckId == deckId)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Id)
            .Select(c => new CardResponseDto
            {
                Id = c.Id,
                DeckId = c.DeckId,
                Question = c.Question,
                Answer = c.Answer,
                VoiceNotes = c.VoiceNotes,
                DisplayOrder = c.DisplayOrder,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CardResponseDto?> GetByIdAsync(int id)
    {
        var card = await _context.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (card == null)
            return null;

        return new CardResponseDto
        {
            Id = card.Id,
            DeckId = card.DeckId,
            Question = card.Question,
            Answer = card.Answer,
            VoiceNotes = card.VoiceNotes,
            DisplayOrder = card.DisplayOrder,
            CreatedAt = card.CreatedAt
        };
    }

    public async Task<CardResponseDto> CreateAsync(CreateCardDto dto)
    {
        var deck = await _context.Decks.FindAsync(dto.DeckId);
        if (deck == null)
        {
            throw new KeyNotFoundException($"Baralho com ID {dto.DeckId} não foi encontrado.");
        }

        var displayOrder = dto.DisplayOrder;
        if (displayOrder <= 0)
        {
            var maxOrder = await _context.Cards
                .Where(c => c.DeckId == dto.DeckId)
                .Select(c => (int?)c.DisplayOrder)
                .MaxAsync() ?? 0;
            displayOrder = maxOrder + 1;
        }

        var card = new Card
        {
            DeckId = dto.DeckId,
            Question = dto.Question.Trim(),
            Answer = dto.Answer.Trim(),
            VoiceNotes = dto.VoiceNotes?.Trim(),
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.Cards.Add(card);
        deck.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Ficha criada com sucesso: ID {CardId} no Baralho {DeckId}", card.Id, deck.Id);

        return new CardResponseDto
        {
            Id = card.Id,
            DeckId = card.DeckId,
            Question = card.Question,
            Answer = card.Answer,
            VoiceNotes = card.VoiceNotes,
            DisplayOrder = card.DisplayOrder,
            CreatedAt = card.CreatedAt
        };
    }

    public async Task<CardResponseDto?> UpdateAsync(int id, UpdateCardDto dto)
    {
        var card = await _context.Cards.Include(c => c.Deck).FirstOrDefaultAsync(c => c.Id == id);
        if (card == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Question))
            card.Question = dto.Question.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Answer))
            card.Answer = dto.Answer.Trim();

        if (dto.VoiceNotes != null)
            card.VoiceNotes = dto.VoiceNotes.Trim();

        if (dto.DisplayOrder.HasValue)
            card.DisplayOrder = dto.DisplayOrder.Value;

        if (card.Deck != null)
            card.Deck.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Ficha atualizada com sucesso: ID {CardId}", card.Id);

        return new CardResponseDto
        {
            Id = card.Id,
            DeckId = card.DeckId,
            Question = card.Question,
            Answer = card.Answer,
            VoiceNotes = card.VoiceNotes,
            DisplayOrder = card.DisplayOrder,
            CreatedAt = card.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var card = await _context.Cards.Include(c => c.Deck).FirstOrDefaultAsync(c => c.Id == id);
        if (card == null)
            return false;

        if (card.Deck != null)
            card.Deck.UpdatedAt = DateTime.UtcNow;

        _context.Cards.Remove(card);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Ficha removida com sucesso: ID {CardId}", id);
        return true;
    }
}
