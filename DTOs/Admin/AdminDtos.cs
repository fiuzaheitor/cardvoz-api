using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Admin;

public class AdminStatsDto
{
    public int Users { get; set; }
    public int Admins { get; set; }
    public int Decks { get; set; }
    public int SharedDecks { get; set; }
    public int Cards { get; set; }
    public int StudySessions { get; set; }
}

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

public class SetAdminDto
{
    public bool IsAdmin { get; set; }
}

/// <summary>
/// Baralho compartilhado inteiro: ao editar, título, descrição e fichas são substituídos.
/// </summary>
public class SharedDeckDto
{
    [Required(ErrorMessage = "O título do baralho é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O título não pode ter mais de 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Adicione pelo menos uma ficha.")]
    [MinLength(1, ErrorMessage = "Adicione pelo menos uma ficha.")]
    [MaxLength(1000, ErrorMessage = "O baralho pode ter no máximo 1000 fichas.")]
    public List<SharedCardDto> Cards { get; set; } = new();
}

public class SharedCardDto
{
    [Required(ErrorMessage = "A pergunta é obrigatória.")]
    [MaxLength(5000, ErrorMessage = "A pergunta não pode ter mais de 5000 caracteres.")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "A resposta é obrigatória.")]
    [MaxLength(5000, ErrorMessage = "A resposta não pode ter mais de 5000 caracteres.")]
    public string Answer { get; set; } = string.Empty;
}
