namespace cardvoz_api.DTOs.Cards;

public class CardResponseDto
{
    public int Id { get; set; }
    public int DeckId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? VoiceNotes { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
