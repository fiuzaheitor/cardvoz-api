using cardvoz_api.DTOs.StudySessions;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace cardvoz_api.Controllers;

[ApiController]
[Route("api/study-sessions")]
[Produces("application/json")]
public class StudySessionsController : ControllerBase
{
    private readonly IStudyService _studyService;
    private readonly ILogger<StudySessionsController> _logger;

    public StudySessionsController(IStudyService studyService, ILogger<StudySessionsController> logger)
    {
        _studyService = studyService;
        _logger = logger;
    }

    /// <summary>
    /// Inicia uma nova sessão de estudos para um baralho (por DeckId ou DeckSlug).
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(StudySessionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudySessionResponseDto>> Start([FromBody] StartSessionDto dto)
    {
        if (!dto.DeckId.HasValue && string.IsNullOrWhiteSpace(dto.DeckSlug))
        {
            return BadRequest(new { message = "É necessário fornecer 'deckId' ou 'deckSlug' para iniciar a sessão." });
        }

        try
        {
            var session = await _studyService.StartSessionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { sessionId = session.Id }, session);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Registra a resposta dada a um flashcard durante uma sessão ativa (com suporte a indicação de uso de voz).
    /// </summary>
    [HttpPost("{sessionId:int}/review")]
    [ProducesResponseType(typeof(StudyReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudyReviewResponseDto>> RecordReview(int sessionId, [FromBody] RecordReviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var review = await _studyService.RecordReviewAsync(sessionId, dto);
            if (review == null)
            {
                return NotFound(new { message = $"Sessão de estudo com ID {sessionId} não foi encontrada." });
            }

            return Ok(review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Finaliza uma sessão de estudos e consolida as métricas finais.
    /// </summary>
    [HttpPost("{sessionId:int}/finish")]
    [ProducesResponseType(typeof(StudySessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudySessionResponseDto>> Finish(int sessionId)
    {
        var session = await _studyService.FinishSessionAsync(sessionId);
        if (session == null)
        {
            return NotFound(new { message = $"Sessão de estudo com ID {sessionId} não foi encontrada." });
        }

        return Ok(session);
    }

    /// <summary>
    /// Obtém os detalhes e histórico de respostas de uma sessão de estudo específica.
    /// </summary>
    [HttpGet("{sessionId:int}")]
    [ProducesResponseType(typeof(StudySessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudySessionResponseDto>> GetById(int sessionId)
    {
        var session = await _studyService.GetSessionByIdAsync(sessionId);
        if (session == null)
        {
            return NotFound(new { message = $"Sessão de estudo com ID {sessionId} não foi encontrada." });
        }

        return Ok(session);
    }

    /// <summary>
    /// Consulta o histórico geral de sessões de estudo realizadas, opcionalmente filtrando por baralho.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<StudySessionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudySessionResponseDto>>> GetHistory(
        [FromQuery] int? deckId,
        [FromQuery] int limit = 20)
    {
        var history = await _studyService.GetHistoryAsync(deckId, limit);
        return Ok(history);
    }
}
