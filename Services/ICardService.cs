using cardvoz_api.DTOs.Cards;

namespace cardvoz_api.Services;

/// <summary>
/// Fichas pertencem a baralhos privados: só o dono do baralho acessa suas fichas.
/// </summary>
public interface ICardService
{
    /// <summary>Retorna null se o baralho não existir ou for de outra pessoa.</summary>
    Task<IEnumerable<CardResponseDto>?> GetByDeckIdAsync(Guid userId, Guid deckId);
    Task<CardResponseDto?> GetByIdAsync(Guid userId, Guid id);
    Task<CardResponseDto> CreateAsync(Guid userId, CreateCardDto dto);
    Task<CardResponseDto?> UpdateAsync(Guid userId, Guid id, UpdateCardDto dto);
    Task<bool> DeleteAsync(Guid userId, Guid id);
}
