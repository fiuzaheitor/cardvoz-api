namespace cardvoz_api.Models;

/// <summary>Código do link de confirmação de e-mail: uso único, 24 horas, só o hash no banco.</summary>
public class EmailConfirmationToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public User? User { get; set; }
}
