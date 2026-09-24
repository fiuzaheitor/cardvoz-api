using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using cardvoz_api.Data;
using cardvoz_api.DTOs.Cards;
using cardvoz_api.DTOs.Decks;
using cardvoz_api.Models;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Services;

public class DeckService : IDeckService
{
    private readonly CardVozDbContext _context;
    private readonly ILogger<DeckService> _logger;

    public DeckService(CardVozDbContext context, ILogger<DeckService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DeckResponseDto>> GetAllAsync(string? category = null, string? search = null)
    {
        var query = _context.Decks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim();
            query = query.Where(d => d.Category.ToLower() == cat.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(term) ||
                                     (d.Description != null && d.Description.ToLower().Contains(term)));
        }

        var decks = await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DeckResponseDto
            {
                Id = d.Id,
                Slug = d.Slug,
                Title = d.Title,
                Category = d.Category,
                Description = d.Description,
                CardCount = d.Cards.Count,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync();

        return decks;
    }

    public async Task<DeckDetailResponseDto?> GetByIdOrSlugAsync(string idOrSlug)
    {
        if (string.IsNullOrWhiteSpace(idOrSlug))
            return null;

        var isNumeric = int.TryParse(idOrSlug, out var numericId);

        var query = _context.Decks
            .AsNoTracking()
            .Include(d => d.Cards.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id));

        Deck? deck;
        if (isNumeric)
        {
            deck = await query.FirstOrDefaultAsync(d => d.Id == numericId);
        }
        else
        {
            deck = await query.FirstOrDefaultAsync(d => d.Slug == idOrSlug.Trim().ToLower());
        }

        if (deck == null)
            return null;

        return new DeckDetailResponseDto
        {
            Id = deck.Id,
            Slug = deck.Slug,
            Title = deck.Title,
            Category = deck.Category,
            Description = deck.Description,
            CardCount = deck.Cards.Count,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt,
            Cards = deck.Cards.Select(c => new CardResponseDto
            {
                Id = c.Id,
                DeckId = c.DeckId,
                Question = c.Question,
                Answer = c.Answer,
                VoiceNotes = c.VoiceNotes,
                DisplayOrder = c.DisplayOrder,
                CreatedAt = c.CreatedAt
            }).ToList()
        };
    }

    public async Task<DeckResponseDto> CreateAsync(CreateDeckDto dto)
    {
        var slug = !string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(dto.Slug)
            : GenerateSlug(dto.Title);

        // Garante unicidade do slug
        slug = await EnsureUniqueSlugAsync(slug);

        var deck = new Deck
        {
            Title = dto.Title.Trim(),
            Slug = slug,
            Category = dto.Category.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Baralho criado com sucesso: ID {Id}, Slug {Slug}", deck.Id, deck.Slug);

        return new DeckResponseDto
        {
            Id = deck.Id,
            Slug = deck.Slug,
            Title = deck.Title,
            Category = deck.Category,
            Description = deck.Description,
            CardCount = 0,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt
        };
    }

    public async Task<DeckResponseDto?> UpdateAsync(int id, UpdateDeckDto dto)
    {
        var deck = await _context.Decks.Include(d => d.Cards).FirstOrDefaultAsync(d => d.Id == id);
        if (deck == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Title))
            deck.Title = dto.Title.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Category))
            deck.Category = dto.Category.Trim();

        if (dto.Description != null)
            deck.Description = dto.Description.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Slug))
        {
            var newSlug = GenerateSlug(dto.Slug);
            if (newSlug != deck.Slug)
            {
                deck.Slug = await EnsureUniqueSlugAsync(newSlug, deck.Id);
            }
        }

        deck.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Baralho atualizado com sucesso: ID {Id}", deck.Id);

        return new DeckResponseDto
        {
            Id = deck.Id,
            Slug = deck.Slug,
            Title = deck.Title,
            Category = deck.Category,
            Description = deck.Description,
            CardCount = deck.Cards.Count,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deck = await _context.Decks.FindAsync(id);
        if (deck == null)
            return false;

        _context.Decks.Remove(deck);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Baralho removido com sucesso: ID {Id}", id);
        return true;
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.Decks
            .AsNoTracking()
            .Select(d => d.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    private static string GenerateSlug(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var clean = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        clean = Regex.Replace(clean, @"[^a-z0-9\s-]", "");
        clean = Regex.Replace(clean, @"\s+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(clean) ? "deck" : clean;
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, int? excludeId = null)
    {
        var slug = baseSlug;
        var counter = 1;

        while (await _context.Decks.AnyAsync(d => d.Slug == slug && (!excludeId.HasValue || d.Id != excludeId.Value)))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        return slug;
    }
}
