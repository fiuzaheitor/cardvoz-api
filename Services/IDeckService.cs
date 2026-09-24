using cardvoz_api.DTOs.Decks;

namespace cardvoz_api.Services;

public interface IDeckService
{
    Task<IEnumerable<DeckResponseDto>> GetAllAsync(string? category = null, string? search = null);
    Task<DeckDetailResponseDto?> GetByIdOrSlugAsync(string idOrSlug);
    Task<DeckResponseDto> CreateAsync(CreateDeckDto dto);
    Task<DeckResponseDto?> UpdateAsync(int id, UpdateDeckDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<string>> GetCategoriesAsync();
}
