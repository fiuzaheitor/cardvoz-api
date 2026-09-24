using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Decks;

public class CreateDeckDto
{
    [Required(ErrorMessage = "O título do baralho é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O título não pode ter mais de 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [MaxLength(100, ErrorMessage = "A categoria não pode ter mais de 100 caracteres.")]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres.")]
    public string? Description { get; set; }

    [MaxLength(150, ErrorMessage = "O slug não pode ter mais de 150 caracteres.")]
    public string? Slug { get; set; }
}
