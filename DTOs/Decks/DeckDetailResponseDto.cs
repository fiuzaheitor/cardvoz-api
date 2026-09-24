using cardvoz_api.DTOs.Cards;

namespace cardvoz_api.DTOs.Decks;

public class DeckDetailResponseDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CardCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<CardResponseDto> Cards { get; set; } = new List<CardResponseDto>();
}
