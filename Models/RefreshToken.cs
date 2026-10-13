using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace cardvoz_api.Models;

/// <summary>
/// Token de renovação de sessão. Só o hash SHA-256 é guardado: quem lê o banco
/// não consegue usar os tokens. Cada uso gera um token novo e revoga o anterior.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required]
    [MaxLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Token que substituiu este na renovação (rastro da rotação).
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}
