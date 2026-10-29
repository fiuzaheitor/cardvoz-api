namespace cardvoz_api.DTOs.Admin;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool HasPassword { get; set; }
    public bool HasGoogle { get; set; }
    public int DeckCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
