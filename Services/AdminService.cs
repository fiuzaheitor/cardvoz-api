using cardvoz_api.Data;
using cardvoz_api.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Services;

public enum AdminChange
{
    Done,
    NotFound,
    /// <summary>Admin tentando tirar o próprio acesso ou excluir a própria conta.</summary>
    Self,
}

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync();
    Task<IEnumerable<AdminUserDto>> GetUsersAsync();
    Task<AdminChange> SetAdminAsync(Guid actingUserId, Guid userId, bool isAdmin);
    Task<AdminChange> DeleteUserAsync(Guid actingUserId, Guid userId);
}

public class AdminService : IAdminService
{
    private readonly CardVozDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(CardVozDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AdminStatsDto> GetStatsAsync() => new()
    {
        Users = await _context.Users.CountAsync(),
        Admins = await _context.Users.CountAsync(u => u.IsAdmin),
        Decks = await _context.Decks.CountAsync(d => d.UserId != null),
        SharedDecks = await _context.Decks.CountAsync(d => d.UserId == null),
        Cards = await _context.Cards.CountAsync(),
        StudySessions = await _context.StudySessions.CountAsync(),
    };

    public async Task<IEnumerable<AdminUserDto>> GetUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                HasPassword = u.PasswordHash != null,
                HasGoogle = u.GoogleSubject != null,
                DeckCount = u.Decks.Count,
                CreatedAt = u.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task<AdminChange> SetAdminAsync(Guid actingUserId, Guid userId, bool isAdmin)
    {
        // Impede o último admin de se trancar fora do painel
        if (actingUserId == userId) return AdminChange.Self;

        var updated = await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.IsAdmin, isAdmin)
                .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));
        if (updated == 0) return AdminChange.NotFound;

        _logger.LogInformation("Admin {ActingUserId} mudou o admin de {UserId} para {IsAdmin}", actingUserId, userId, isAdmin);
        return AdminChange.Done;
    }

    public async Task<AdminChange> DeleteUserAsync(Guid actingUserId, Guid userId)
    {
        if (actingUserId == userId) return AdminChange.Self;

        // Baralhos, fichas, sessões de estudo e de login saem juntos (ON DELETE CASCADE)
        var deleted = await _context.Users.Where(u => u.Id == userId).ExecuteDeleteAsync();
        if (deleted == 0) return AdminChange.NotFound;

        _logger.LogWarning("Admin {ActingUserId} excluiu o usuário {UserId}", actingUserId, userId);
        return AdminChange.Done;
    }
}
