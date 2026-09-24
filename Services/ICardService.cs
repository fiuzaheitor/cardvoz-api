using cardvoz_api.DTOs.Cards;

namespace cardvoz_api.Services;

public interface ICardService
{
    Task<IEnumerable<CardResponseDto>> GetByDeckIdAsync(int deckId);
    Task<CardResponseDto?> GetByIdAsync(int id);
    Task<CardResponseDto> CreateAsync(CreateCardDto dto);
    Task<CardResponseDto?> UpdateAsync(int id, UpdateCardDto dto);
    Task<bool> DeleteAsync(int id);
}
