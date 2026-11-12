namespace cardvoz_api.Models;

/// <summary>
/// Código de "Esqueci minha senha": uso único, válido por 1 hora. Só o hash fica no
/// banco, como nos tokens de renovação.
/// </summary>
public class PasswordResetToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public User? User { get; set; }
}
