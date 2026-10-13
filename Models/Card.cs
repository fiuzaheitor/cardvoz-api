using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace cardvoz_api.Models;

/// <summary>
/// Representa uma ficha individual de estudo (flashcard).
/// </summary>
public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeckId { get; set; }

    [Required]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Notas de voz, pronúncia alternativa ou palavras-chave esperadas para reconhecimento de voz.
    /// </summary>
    [MaxLength(500)]
    public string? VoiceNotes { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Deck? Deck { get; set; }
}
