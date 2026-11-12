using cardvoz_api.DTOs.Cards;

namespace cardvoz_api.DTOs.Decks;

public class DeckDetailResponseDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CardCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Idioma das perguntas (ex.: "en"). Nulo = português (fica fora do JSON).</summary>
    public string? QuestionLanguage { get; set; }

    /// <summary>Idioma das respostas. Nulo = português.</summary>
    public string? AnswerLanguage { get; set; }

    /// <summary>Última vez que o baralho foi estudado (nulo = nunca).</summary>
    public DateTime? LastStudiedAt { get; set; }
    public IEnumerable<CardResponseDto> Cards { get; set; } = new List<CardResponseDto>();
}
