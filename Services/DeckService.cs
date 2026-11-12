using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using cardvoz_api.Data;
using cardvoz_api.DTOs.Admin;
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

    public async Task<IEnumerable<DeckResponseDto>> GetAllAsync(Guid userId, string? category = null, string? search = null)
    {
        var query = _context.Decks.AsNoTracking().Where(d => d.UserId == userId);

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
                UpdatedAt = d.UpdatedAt,
                QuestionLanguage = d.QuestionLanguage,
                AnswerLanguage = d.AnswerLanguage,
                LastStudiedAt = d.LastStudiedAt,
                SourceDeckId = d.SourceDeckId
            })
            .ToListAsync();

        return decks;
    }

    public async Task<DeckDetailResponseDto?> GetByIdOrSlugAsync(Guid userId, string idOrSlug)
    {
        if (string.IsNullOrWhiteSpace(idOrSlug))
            return null;

        var query = _context.Decks
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .Include(d => d.Cards.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreatedAt));

        Deck? deck;
        if (Guid.TryParse(idOrSlug, out var id))
        {
            deck = await query.FirstOrDefaultAsync(d => d.Id == id);
        }
        else
        {
            var slug = idOrSlug.Trim().ToLower();
            deck = await query.FirstOrDefaultAsync(d => d.Slug == slug);
        }

        return deck == null ? null : ToDetail(deck);
    }

    public Task<DeckResponseDto> CreateAsync(Guid userId, CreateDeckDto dto) =>
        WithUserDecksLockAsync(userId, () => CreateCoreAsync(userId, dto));

    private async Task<DeckResponseDto> CreateCoreAsync(Guid userId, CreateDeckDto dto)
    {
        var slug = !string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(dto.Slug)
            : GenerateSlug(dto.Title);

        // Garante unicidade do slug entre os baralhos da pessoa
        slug = await EnsureUniqueSlugAsync(userId, slug);

        var deck = new Deck
        {
            UserId = userId,
            Title = dto.Title.Trim(),
            Slug = slug,
            Category = dto.Category.Trim(),
            Description = dto.Description?.Trim(),
            QuestionLanguage = Language(dto.QuestionLanguage),
            AnswerLanguage = Language(dto.AnswerLanguage),
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
            UpdatedAt = deck.UpdatedAt,
            QuestionLanguage = deck.QuestionLanguage,
            AnswerLanguage = deck.AnswerLanguage
        };
    }

    /// <summary>
    /// Baralho e fichas num SaveChanges só: o EF grava tudo numa transação, então
    /// ou o baralho chega inteiro ou nada é salvo.
    /// </summary>
    public Task<(CreateOutcome Outcome, DeckDetailResponseDto? Deck)> ImportAsync(Guid userId, ImportDeckDto dto) =>
        WithUserDecksLockAsync(userId, () => ImportCoreAsync(userId, dto));

    private async Task<(CreateOutcome Outcome, DeckDetailResponseDto? Deck)> ImportCoreAsync(Guid userId, ImportDeckDto dto)
    {
        if (dto.Id.HasValue)
        {
            // O mesmo pedido de novo (a resposta anterior se perdeu): devolve o que já foi salvo
            var existing = await _context.Decks
                .AsNoTracking()
                .Include(d => d.Cards.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreatedAt))
                .FirstOrDefaultAsync(d => d.Id == dto.Id.Value);
            if (existing != null)
            {
                return existing.UserId == userId
                    ? (CreateOutcome.Existing, ToDetail(existing))
                    : (CreateOutcome.Conflict, null);
            }
        }

        var now = DateTime.UtcNow;
        var deck = new Deck
        {
            UserId = userId,
            Title = dto.Title.Trim(),
            Slug = await EnsureUniqueSlugAsync(userId, GenerateSlug(dto.Title)),
            Category = dto.Category.Trim(),
            Description = dto.Description?.Trim(),
            QuestionLanguage = Language(dto.QuestionLanguage),
            AnswerLanguage = Language(dto.AnswerLanguage),
            CreatedAt = now
        };
        if (dto.Id.HasValue)
            deck.Id = dto.Id.Value;

        deck.Cards = NewCards(deck.Id, dto.Cards, 1, now);
        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Baralho importado: ID {Id} com {Count} fichas", deck.Id, deck.Cards.Count);
        return (CreateOutcome.Created, ToDetail(deck));
    }

    /// <summary>Várias fichas no fim de um baralho que já existe, numa transação.</summary>
    public async Task<DeckDetailResponseDto?> AddCardsBatchAsync(Guid userId, Guid deckId, BatchCardsDto dto)
    {
        var deck = await _context.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId);
        if (deck == null)
            return null;

        var now = DateTime.UtcNow;
        var start = deck.Cards.Select(c => c.DisplayOrder).DefaultIfEmpty(0).Max() + 1;
        _context.Cards.AddRange(NewCards(deck.Id, dto.Cards, start, now));
        deck.UpdatedAt = now;
        await _context.SaveChangesAsync();

        deck.Cards = deck.Cards.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreatedAt).ToList();
        return ToDetail(deck);
    }

    /// <summary>
    /// Baralhos criados, alterados ou estudados depois de "since", com as fichas, e os
    /// ids de todos (o banco não guarda exclusões). O site atualiza as cópias do
    /// aparelho com uma chamada só.
    /// </summary>
    public async Task<DeckChangesDto> GetChangesAsync(Guid userId, DateTime? since)
    {
        var serverTime = DateTime.UtcNow;
        var own = _context.Decks.AsNoTracking().Where(d => d.UserId == userId);

        var changed = own;
        if (since.HasValue)
        {
            // Folga: o que foi gravado no mesmo instante da consulta anterior pode ter
            // horário um pouco antes do serverTime devolvido. Um baralho a mais não faz mal.
            var from = since.Value.ToUniversalTime().AddSeconds(-5);
            changed = own.Where(d => d.CreatedAt > from || d.UpdatedAt > from || d.LastStudiedAt > from);
        }

        var decks = await changed
            .Include(d => d.Cards.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreatedAt))
            .AsSplitQuery()
            .ToListAsync();

        return new DeckChangesDto
        {
            Decks = decks.Select(ToDetail).ToList(),
            AllIds = await own.Select(d => d.Id).ToListAsync(),
            ServerTime = serverTime
        };
    }

    /// <summary>Código de idioma já validado pelo DTO; vazio = português (nulo).</summary>
    private static string? Language(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<Card> NewCards(Guid deckId, IEnumerable<ImportCardDto> cards, int startOrder, DateTime now)
    {
        return cards.Select((c, i) =>
        {
            var card = new Card
            {
                DeckId = deckId,
                Question = c.Question.Trim(),
                Answer = c.Answer.Trim(),
                DisplayOrder = startOrder + i,
                CreatedAt = now
            };
            card.SetHistory(c.CorrectCount, c.IncorrectCount, c.LastAnsweredAt, c.LastWasCorrect, c.ReviewLevel, c.NextReviewAt);
            return card;
        }).ToList();
    }

    public Task<DeckResponseDto?> UpdateAsync(Guid userId, Guid id, UpdateDeckDto dto) =>
        WithUserDecksLockAsync(userId, () => UpdateCoreAsync(userId, id, dto));

    private async Task<DeckResponseDto?> UpdateCoreAsync(Guid userId, Guid id, UpdateDeckDto dto)
    {
        var deck = await _context.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
        if (deck == null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Title))
            deck.Title = dto.Title.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Category))
            deck.Category = dto.Category.Trim();

        if (dto.Description != null)
            deck.Description = dto.Description.Trim();

        // Ausente ou nulo: não muda. Texto vazio: volta para português (nulo)
        if (dto.QuestionLanguage != null)
            deck.QuestionLanguage = Language(dto.QuestionLanguage);
        if (dto.AnswerLanguage != null)
            deck.AnswerLanguage = Language(dto.AnswerLanguage);

        if (!string.IsNullOrWhiteSpace(dto.Slug))
        {
            var newSlug = GenerateSlug(dto.Slug);
            if (newSlug != deck.Slug)
            {
                deck.Slug = await EnsureUniqueSlugAsync(userId, newSlug, deck.Id);
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
            UpdatedAt = deck.UpdatedAt,
            QuestionLanguage = deck.QuestionLanguage,
            AnswerLanguage = deck.AnswerLanguage,
            SourceDeckId = deck.SourceDeckId
        };
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var deck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
        if (deck == null)
            return false;

        _context.Decks.Remove(deck);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Baralho removido com sucesso: ID {Id}", id);
        return true;
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(Guid userId)
    {
        return await _context.Decks
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .Select(d => d.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    // === Baralhos compartilhados: sem dono, geridos pelo painel de admin ===

    private const string SharedCategory = "Geral";

    /// <summary>Tamanho da coluna "Slug" no banco.</summary>
    private const int MaxSlugLength = 150;

    public async Task<IEnumerable<DeckResponseDto>> GetSharedAsync()
    {
        return await _context.Decks
            .AsNoTracking()
            .Where(d => d.UserId == null)
            .OrderBy(d => d.Title)
            .Select(d => new DeckResponseDto
            {
                Id = d.Id,
                Slug = d.Slug,
                Title = d.Title,
                Category = d.Category,
                Description = d.Description,
                CardCount = d.Cards.Count,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                QuestionLanguage = d.QuestionLanguage,
                AnswerLanguage = d.AnswerLanguage
            })
            .ToListAsync();
    }

    public async Task<DeckDetailResponseDto?> GetSharedByIdAsync(Guid id)
    {
        var deck = await _context.Decks
            .AsNoTracking()
            .Include(d => d.Cards.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreatedAt))
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == null);

        return deck == null ? null : ToDetail(deck);
    }

    public async Task<DeckResponseDto> CreateSharedAsync(SharedDeckDto dto)
    {
        var deck = new Deck
        {
            UserId = null,
            Title = dto.Title.Trim(),
            // Sem dono não há conflito de slug; cada cópia ganha um slug único da pessoa
            Slug = GenerateSlug(dto.Title),
            Category = SharedCategory,
            Description = dto.Description?.Trim(),
            QuestionLanguage = Language(dto.QuestionLanguage),
            AnswerLanguage = Language(dto.AnswerLanguage),
            CreatedAt = DateTime.UtcNow
        };

        _context.Decks.Add(deck);
        AddSharedCards(deck.Id, dto.Cards);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Baralho compartilhado criado: ID {Id}", deck.Id);
        return ToSummary(deck, dto.Cards.Count);
    }

    public async Task<DeckResponseDto?> UpdateSharedAsync(Guid id, SharedDeckDto dto)
    {
        var deck = await _context.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == null);
        if (deck == null)
            return null;

        deck.Title = dto.Title.Trim();
        deck.Slug = GenerateSlug(dto.Title);
        deck.Description = dto.Description?.Trim();
        deck.QuestionLanguage = Language(dto.QuestionLanguage);
        deck.AnswerLanguage = Language(dto.AnswerLanguage);
        deck.UpdatedAt = DateTime.UtcNow;

        // As fichas são substituídas: as cópias já feitas são independentes e não mudam
        _context.Cards.RemoveRange(deck.Cards);
        AddSharedCards(deck.Id, dto.Cards);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Baralho compartilhado atualizado: ID {Id}", deck.Id);
        return ToSummary(deck, dto.Cards.Count);
    }

    public async Task<bool> DeleteSharedAsync(Guid id)
    {
        // As fichas saem junto (ON DELETE CASCADE no banco)
        var deleted = await _context.Decks.Where(d => d.Id == id && d.UserId == null).ExecuteDeleteAsync();
        return deleted > 0;
    }

    public Task<DeckResponseDto?> CopySharedAsync(Guid userId, Guid sharedId) =>
        WithUserDecksLockAsync(userId, () => CopySharedCoreAsync(userId, sharedId));

    private async Task<DeckResponseDto?> CopySharedCoreAsync(Guid userId, Guid sharedId)
    {
        var shared = await _context.Decks
            .AsNoTracking()
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == sharedId && d.UserId == null);
        if (shared == null)
            return null;

        var now = DateTime.UtcNow;
        var deck = new Deck
        {
            UserId = userId,
            Title = shared.Title,
            Slug = await EnsureUniqueSlugAsync(userId, shared.Slug),
            Category = shared.Category,
            Description = shared.Description,
            SourceDeckId = shared.Id,
            QuestionLanguage = shared.QuestionLanguage,
            AnswerLanguage = shared.AnswerLanguage,
            CreatedAt = now
        };

        _context.Decks.Add(deck);
        _context.Cards.AddRange(shared.Cards.Select(c => new Card
        {
            DeckId = deck.Id,
            Question = c.Question,
            Answer = c.Answer,
            DisplayOrder = c.DisplayOrder,
            CreatedAt = now
        }));
        await _context.SaveChangesAsync();

        _logger.LogInformation("Baralho compartilhado {SharedId} copiado: ID {Id}", sharedId, deck.Id);
        return ToSummary(deck, shared.Cards.Count);
    }

    private void AddSharedCards(Guid deckId, IEnumerable<SharedCardDto> cards)
    {
        var now = DateTime.UtcNow;
        _context.Cards.AddRange(cards.Select((c, i) => new Card
        {
            DeckId = deckId,
            Question = c.Question.Trim(),
            Answer = c.Answer.Trim(),
            DisplayOrder = i,
            CreatedAt = now
        }));
    }

    private static DeckResponseDto ToSummary(Deck deck, int cardCount) => new()
    {
        Id = deck.Id,
        Slug = deck.Slug,
        Title = deck.Title,
        Category = deck.Category,
        Description = deck.Description,
        CardCount = cardCount,
        CreatedAt = deck.CreatedAt,
        UpdatedAt = deck.UpdatedAt,
        QuestionLanguage = deck.QuestionLanguage,
        AnswerLanguage = deck.AnswerLanguage,
        LastStudiedAt = deck.LastStudiedAt,
        SourceDeckId = deck.SourceDeckId
    };

    private static DeckDetailResponseDto ToDetail(Deck deck) => new()
    {
        Id = deck.Id,
        Slug = deck.Slug,
        Title = deck.Title,
        Category = deck.Category,
        Description = deck.Description,
        CardCount = deck.Cards.Count,
        CreatedAt = deck.CreatedAt,
        UpdatedAt = deck.UpdatedAt,
        QuestionLanguage = deck.QuestionLanguage,
        AnswerLanguage = deck.AnswerLanguage,
        LastStudiedAt = deck.LastStudiedAt,
        SourceDeckId = deck.SourceDeckId,
        Cards = deck.Cards.Select(c => new CardResponseDto
        {
            Id = c.Id,
            DeckId = c.DeckId,
            Question = c.Question,
            Answer = c.Answer,
            VoiceNotes = c.VoiceNotes,
            DisplayOrder = c.DisplayOrder,
            CreatedAt = c.CreatedAt,
            CorrectCount = c.CorrectCount,
            IncorrectCount = c.IncorrectCount,
            LastAnsweredAt = c.LastAnsweredAt,
            LastWasCorrect = c.LastWasCorrect,
            ReviewLevel = c.ReviewLevel,
            NextReviewAt = c.NextReviewAt
        }).ToList()
    };

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
        if (clean.Length > MaxSlugLength)
            clean = clean[..MaxSlugLength].TrimEnd('-');
        return string.IsNullOrWhiteSpace(clean) ? "deck" : clean;
    }

    /// <summary>
    /// Endereços fixos de /api/decks/...: um baralho com esse slug não abriria por slug,
    /// porque o segmento fixo tem prioridade sobre {idOrSlug}.
    /// </summary>
    private static readonly HashSet<string> ReservedSlugs = new() { "changes", "import", "categories" };

    /// <summary>
    /// Uma operação por vez nos baralhos da pessoa: o slug conferido como livre não é
    /// pego por outro pedido antes de gravar, e o mesmo import repetido ao mesmo tempo
    /// devolve o baralho salvo em vez de bater na chave primária (500).
    /// </summary>
    private async Task<T> WithUserDecksLockAsync<T>(Guid userId, Func<Task<T>> work)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _context.LockUserDecksAsync(userId);
        var result = await work();
        await transaction.CommitAsync();
        return result;
    }

    private async Task<string> EnsureUniqueSlugAsync(Guid userId, string baseSlug, Guid? excludeId = null)
    {
        var slug = baseSlug;
        var counter = 1;

        while (ReservedSlugs.Contains(slug) || await _context.Decks.AnyAsync(d =>
                   d.UserId == userId && d.Slug == slug && (!excludeId.HasValue || d.Id != excludeId.Value)))
        {
            // O sufixo cabe nos 150 caracteres da coluna cortando o fim do slug
            var suffix = $"-{counter++}";
            slug = baseSlug[..Math.Min(baseSlug.Length, MaxSlugLength - suffix.Length)].TrimEnd('-') + suffix;
        }

        return slug;
    }
}
