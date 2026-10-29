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
    /// Nulo em contas que entram só com o Google.
    /// </summary>
    [JsonIgnore]
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Identificador da conta Google (claim "sub"), único. Ao contrário do e-mail,
    /// não muda se a pessoa trocar o e-mail da conta Google.
    /// </summary>
    [MaxLength(255)]
    [JsonIgnore]
    public string? GoogleSubject { get; set; }

    /// <summary>
    /// Tentativas de login erradas seguidas; zera ao acertar.
    /// </summary>
    public int FailedLoginCount { get; set; }

    /// <summary>
    /// Enquanto estiver no futuro, o login fica bloqueado.
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// Pode entrar no painel de admin (gerenciar usuários e baralhos compartilhados).
    /// </summary>
    public bool IsAdmin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    public ICollection<Deck> Decks { get; set; } = new List<Deck>();

    [JsonIgnore]
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
