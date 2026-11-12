using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Admin;

/// <summary>
/// Baralho compartilhado inteiro: ao editar, título, descrição e fichas são substituídos.
/// </summary>
public class SharedDeckDto : IValidatableObject
{
    [Required(ErrorMessage = "O título do baralho é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O título não pode ter mais de 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres.")]
    public string? Description { get; set; }

    /// <summary>Idioma das perguntas (ex.: "en"). Nulo = português. Vai para o atributo lang do site.</summary>
    [MaxLength(10, ErrorMessage = "O código de idioma pode ter até 10 caracteres.")]
    [RegularExpression(@"^([a-z]{2,3}(-[A-Z]{2})?)?$", ErrorMessage = "Use um código de idioma, como en, es ou pt-BR.")]
    public string? QuestionLanguage { get; set; }

    /// <summary>Idioma das respostas. Nulo = português.</summary>
    [MaxLength(10, ErrorMessage = "O código de idioma pode ter até 10 caracteres.")]
    [RegularExpression(@"^([a-z]{2,3}(-[A-Z]{2})?)?$", ErrorMessage = "Use um código de idioma, como en, es ou pt-BR.")]
    public string? AnswerLanguage { get; set; }

    [Required(ErrorMessage = "Adicione pelo menos uma ficha.")]
    [MinLength(1, ErrorMessage = "Adicione pelo menos uma ficha.")]
    [MaxLength(1000, ErrorMessage = "O baralho pode ter no máximo 1000 fichas.")]
    public List<SharedCardDto> Cards { get; set; } = new();

    // As anotações não olham dentro da lista: um item null passaria e derrubaria a API
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Cards.Any(c => c == null))
            yield return new ValidationResult("Todas as fichas precisam de pergunta e resposta.", new[] { nameof(Cards) });
    }
}
