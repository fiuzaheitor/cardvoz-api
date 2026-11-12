namespace cardvoz_api.DTOs.Auth;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }

    /// <summary>A conta tem senha (o site pede a senha para excluir a conta; quem entra só com o Google não tem).</summary>
    public bool HasPassword { get; set; }
    public DateTime CreatedAt { get; set; }
}
