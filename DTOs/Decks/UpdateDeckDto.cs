using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Decks;

public class UpdateDeckDto
{
    [MaxLength(200, ErrorMessage = "O título não pode ter mais de 200 caracteres.")]
    public string? Title { get; set; }

    [MaxLength(100, ErrorMessage = "A categoria não pode ter mais de 100 caracteres.")]
    public string? Category { get; set; }

    [MaxLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres.")]
    public string? Description { get; set; }

    [MaxLength(150, ErrorMessage = "O slug não pode ter mais de 150 caracteres.")]
    public string? Slug { get; set; }

    /// <summary>Idioma das perguntas (ex.: "en"). Nulo = português. Vai para o atributo lang do site.</summary>
    [MaxLength(10, ErrorMessage = "O código de idioma pode ter até 10 caracteres.")]
    [RegularExpression(@"^([a-z]{2,3}(-[A-Z]{2})?)?$", ErrorMessage = "Use um código de idioma, como en, es ou pt-BR.")]
    public string? QuestionLanguage { get; set; }

    /// <summary>Idioma das respostas. Nulo = português.</summary>
    [MaxLength(10, ErrorMessage = "O código de idioma pode ter até 10 caracteres.")]
    [RegularExpression(@"^([a-z]{2,3}(-[A-Z]{2})?)?$", ErrorMessage = "Use um código de idioma, como en, es ou pt-BR.")]
    public string? AnswerLanguage { get; set; }
}
