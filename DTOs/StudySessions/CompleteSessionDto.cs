using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.StudySessions;

/// <summary>
/// Sessão de estudo inteira numa chamada (POST /api/study-sessions/complete). Pode
/// ter sido feita sem internet, horas antes: os horários vêm do aparelho.
/// </summary>
public class CompleteSessionDto
{
    /// <summary>
    /// Id gerado no aparelho (opcional). O mesmo pedido chegando duas vezes não cria
    /// outra sessão nem soma os acertos e erros de novo: responde 200 com a salva.
    /// </summary>
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "O baralho é obrigatório.")]
    public Guid DeckId { get; set; }

    [Required(ErrorMessage = "O início da sessão é obrigatório.")]
    public DateTime StartedAt { get; set; }

    [Required(ErrorMessage = "O fim da sessão é obrigatório.")]
    public DateTime CompletedAt { get; set; }

    [Required(ErrorMessage = "Envie as respostas.")]
    [MaxLength(1000, ErrorMessage = "Envie no máximo 1000 respostas por sessão.")]
    public List<CompleteAnswerDto> Answers { get; set; } = new();
}

public class CompleteAnswerDto
{
    [Required(ErrorMessage = "O identificador da ficha é obrigatório.")]
    public Guid CardId { get; set; }

    public bool WasCorrect { get; set; }

    public DateTime AnsweredAt { get; set; }
}
