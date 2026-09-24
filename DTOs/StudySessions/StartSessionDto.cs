namespace cardvoz_api.DTOs.StudySessions;

public class StartSessionDto
{
    /// <summary>
    /// Identificador numérico do baralho a ser estudado (opcional se DeckSlug for fornecido).
    /// </summary>
    public int? DeckId { get; set; }

    /// <summary>
    /// Slug do baralho a ser estudado (opcional se DeckId for fornecido).
    /// </summary>
    public string? DeckSlug { get; set; }
}
