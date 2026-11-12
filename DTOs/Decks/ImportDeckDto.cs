using System.ComponentModel.DataAnnotations;

namespace cardvoz_api.DTOs.Decks;

/// <summary>
/// Baralho com todas as fichas numa chamada (POST /api/decks/import). Usado pelo
/// site ao importar um arquivo e ao levar para a conta um baralho do aparelho.
/// </summary>
public class ImportDeckDto
{
    /// <summary>
    /// Id gerado no aparelho (opcional). O mesmo pedido chegando duas vezes (internet
    /// que caiu depois de a API salvar) não cria outro baralho: responde 200 com o salvo.
    /// </summary>
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "O título do baralho é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O título não pode ter mais de 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [MaxLength(100, ErrorMessage = "A categoria não pode ter mais de 100 caracteres.")]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres.")]
    public string? Description { get; set; }

    /// <summary>Até 1000 fichas, o mesmo limite da importação do site.</summary>
    [MaxLength(1000, ErrorMessage = "Envie no máximo 1000 fichas por vez.")]
    public List<ImportCardDto> Cards { get; set; } = new();
}

/// <summary>Ficha da importação, com os acertos e erros que já tinha no aparelho (opcionais).</summary>
public class ImportCardDto
{
    [Required(ErrorMessage = "A pergunta é obrigatória.")]
    [MaxLength(5000, ErrorMessage = "A pergunta não pode ter mais de 5000 caracteres.")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "A resposta é obrigatória.")]
    [MaxLength(5000, ErrorMessage = "A resposta não pode ter mais de 5000 caracteres.")]
    public string Answer { get; set; } = string.Empty;

    [Range(0, 100000, ErrorMessage = "A quantidade de acertos precisa estar entre 0 e 100000.")]
    public int CorrectCount { get; set; } = 0;

    [Range(0, 100000, ErrorMessage = "A quantidade de erros precisa estar entre 0 e 100000.")]
    public int IncorrectCount { get; set; } = 0;

    public DateTime? LastAnsweredAt { get; set; }

    public bool? LastWasCorrect { get; set; }
}

/// <summary>Várias fichas para um baralho que já existe (POST /api/decks/{id}/cards/batch).</summary>
public class BatchCardsDto
{
    [Required(ErrorMessage = "Envie as fichas.")]
    [MinLength(1, ErrorMessage = "Envie pelo menos uma ficha.")]
    [MaxLength(1000, ErrorMessage = "Envie no máximo 1000 fichas por vez.")]
    public List<ImportCardDto> Cards { get; set; } = new();
}

/// <summary>Resposta de GET /api/decks/changes.</summary>
public class DeckChangesDto
{
    /// <summary>Baralhos criados, alterados ou estudados depois de "since", com as fichas.</summary>
    public List<DeckDetailResponseDto> Decks { get; set; } = new();

    /// <summary>Ids de todos os baralhos atuais: o que não está aqui foi excluído.</summary>
    public List<Guid> AllIds { get; set; } = new();

    /// <summary>Horário do servidor na consulta: o "since" da próxima vez.</summary>
    public DateTime ServerTime { get; set; }
}

/// <summary>Resultado de uma criação que aceita o mesmo pedido duas vezes.</summary>
public enum CreateOutcome
{
    Created,
    /// <summary>Já existia com esse id, da mesma pessoa: nada novo foi criado.</summary>
    Existing,
    /// <summary>O id já é de outra pessoa.</summary>
    Conflict,
    /// <summary>O baralho não existe ou não é da pessoa.</summary>
    NotFound
}
