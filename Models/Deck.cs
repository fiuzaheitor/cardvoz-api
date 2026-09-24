using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace cardvoz_api.Models;

/// <summary>
/// Representa uma coleção organizada de flashcards (baralho de estudo).
/// </summary>
public class Deck
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    public ICollection<Card> Cards { get; set; } = new List<Card>();

    [JsonIgnore]
    public ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
}
