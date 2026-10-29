using cardvoz_api.Data;
using cardvoz_api.DTOs.Admin;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Services;

public class AdminService : IAdminService
{
    /// <summary>Chave da trava do Postgres que põe as mudanças de admin em fila.</summary>
    private const long AdminChangesLock = 7_347_101;

    private readonly CardVozDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(CardVozDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Uma ida ao banco só, em vez de seis contagens em sequência
    public async Task<AdminStatsDto> GetStatsAsync() =>
        await _context.Database.SqlQuery<AdminStatsDto>($"""
            SELECT
                (SELECT count(*) FROM "Users")::int AS "Users",
                (SELECT count(*) FROM "Users" WHERE "IsAdmin")::int AS "Admins",
                (SELECT count(*) FROM "Decks" WHERE "UserId" IS NOT NULL)::int AS "Decks",
                (SELECT count(*) FROM "Decks" WHERE "UserId" IS NULL)::int AS "SharedDecks",
                (SELECT count(*) FROM "Cards")::int AS "Cards",
                (SELECT count(*) FROM "StudySessions")::int AS "StudySessions"
            """).SingleAsync();

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
        var result = await ChangeAsync(actingUserId, userId, () => _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.IsAdmin, isAdmin)
                .SetProperty(u => u.UpdatedAt, DateTime.UtcNow)));

        if (result == AdminChange.Done)
            _logger.LogInformation("Admin {ActingUserId} mudou o admin de {UserId} para {IsAdmin}", actingUserId, userId, isAdmin);
        return result;
    }

    public async Task<AdminChange> DeleteUserAsync(Guid actingUserId, Guid userId)
    {
        // Baralhos, fichas, sessões de estudo e de login saem juntos (ON DELETE CASCADE)
        var result = await ChangeAsync(actingUserId, userId,
            () => _context.Users.Where(u => u.Id == userId).ExecuteDeleteAsync());

        if (result == AdminChange.Done)
            _logger.LogWarning("Admin {ActingUserId} excluiu o usuário {UserId}", actingUserId, userId);
        return result;
    }

    /// <summary>
    /// Mudanças de admin uma de cada vez, e só se quem pede ainda for admin. Como
    /// ninguém muda a si mesmo, quem pede continua admin: nunca sobra zero admins,
    /// nem com dois admins tirando o acesso um do outro ao mesmo tempo.
    /// </summary>
    private async Task<AdminChange> ChangeAsync(Guid actingUserId, Guid userId, Func<Task<int>> change)
    {
        if (actingUserId == userId) return AdminChange.Self;

        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({AdminChangesLock})");

        if (!await _context.Users.AnyAsync(u => u.Id == actingUserId && u.IsAdmin)) return AdminChange.NotAdmin;
        if (await change() == 0) return AdminChange.NotFound;

        await transaction.CommitAsync();
        return AdminChange.Done;
    }
}
