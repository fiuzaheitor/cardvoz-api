using cardvoz_api.DTOs.Decks;

namespace cardvoz_api.Services;

/// <summary>
/// Baralhos são privados: todas as operações recebem o ID de quem está logado e
/// só enxergam os baralhos dessa pessoa.
/// </summary>
public interface IDeckService
{
    Task<IEnumerable<DeckResponseDto>> GetAllAsync(Guid userId, string? category = null, string? search = null);
    Task<DeckDetailResponseDto?> GetByIdOrSlugAsync(Guid userId, string idOrSlug);
    Task<DeckResponseDto> CreateAsync(Guid userId, CreateDeckDto dto);
    Task<DeckResponseDto?> UpdateAsync(Guid userId, Guid id, UpdateDeckDto dto);
    Task<bool> DeleteAsync(Guid userId, Guid id);
    Task<IEnumerable<string>> GetCategoriesAsync(Guid userId);
}
