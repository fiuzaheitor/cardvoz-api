using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace cardvoz_api.Models;

/// <summary>
/// Conta de uma pessoa usuária do CardVoz.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// E-mail já normalizado (sem espaços nas pontas e em minúsculas), único.
    /// </summary>
    [Required]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash da senha (PBKDF2 do ASP.NET Identity, com salt). A senha nunca é guardada.
    /// </summary>
    [Required]
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Tentativas de login erradas seguidas; zera ao acertar.
    /// </summary>
    public int FailedLoginCount { get; set; }

    /// <summary>
    /// Enquanto estiver no futuro, o login fica bloqueado.
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    public ICollection<Deck> Decks { get; set; } = new List<Deck>();

    [JsonIgnore]
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
