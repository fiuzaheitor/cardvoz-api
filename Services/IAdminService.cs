using cardvoz_api.DTOs.Admin;

namespace cardvoz_api.Services;

public enum AdminChange
{
    Done,
    NotFound,
    /// <summary>Admin tentando tirar o próprio acesso ou excluir a própria conta.</summary>
    Self,
    /// <summary>Quem pediu deixou de ser admin enquanto esperava a vez.</summary>
    NotAdmin,
}

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync();
    Task<IEnumerable<AdminUserDto>> GetUsersAsync();
    Task<AdminChange> SetAdminAsync(Guid actingUserId, Guid userId, bool isAdmin);
    Task<AdminChange> DeleteUserAsync(Guid actingUserId, Guid userId);
}
