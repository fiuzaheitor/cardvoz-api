using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace cardvoz_api.Models;

/// <summary>
/// Representa uma coleção organizada de flashcards (baralho de estudo).
/// </summary>
public class Deck
{
    /// <summary>
    /// Identificador aleatório (UUID v4), impossível de adivinhar a partir de outros IDs.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Dono do baralho. Só ele pode ver, estudar, editar e excluir.
    /// Nulo = baralho compartilhado, criado pelo painel de admin: como toda consulta
    /// filtra pelo dono, ele não aparece para ninguém; cada pessoa adiciona uma cópia.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Baralho compartilhado de onde esta cópia veio. Vira nulo se o compartilhado for excluído.
    /// </summary>
    public Guid? SourceDeckId { get; set; }

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
    public User? User { get; set; }

    [JsonIgnore]
    public ICollection<Card> Cards { get; set; } = new List<Card>();

    [JsonIgnore]
    public ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
}
