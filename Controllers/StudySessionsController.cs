using cardvoz_api.Auth;
using cardvoz_api.DTOs.StudySessions;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cardvoz_api.Controllers;

/// <summary>
/// Sessões de estudo dos baralhos da pessoa logada. Sessões de outras pessoas respondem 404.
/// </summary>
[ApiController]
[Authorize]
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
            var session = await _studyService.StartSessionAsync(User.GetUserId(), dto);
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
    [HttpPost("{sessionId:guid}/review")]
    [ProducesResponseType(typeof(StudyReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudyReviewResponseDto>> RecordReview(Guid sessionId, [FromBody] RecordReviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var review = await _studyService.RecordReviewAsync(User.GetUserId(), sessionId, dto);
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
    [HttpPost("{sessionId:guid}/finish")]
    [ProducesResponseType(typeof(StudySessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudySessionResponseDto>> Finish(Guid sessionId)
    {
        var session = await _studyService.FinishSessionAsync(User.GetUserId(), sessionId);
        if (session == null)
        {
            return NotFound(new { message = $"Sessão de estudo com ID {sessionId} não foi encontrada." });
        }

        return Ok(session);
    }

    /// <summary>
    /// Obtém os detalhes e histórico de respostas de uma sessão de estudo específica.
    /// </summary>
    [HttpGet("{sessionId:guid}")]
    [ProducesResponseType(typeof(StudySessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudySessionResponseDto>> GetById(Guid sessionId)
    {
        var session = await _studyService.GetSessionByIdAsync(User.GetUserId(), sessionId);
        if (session == null)
        {
            return NotFound(new { message = $"Sessão de estudo com ID {sessionId} não foi encontrada." });
        }

        return Ok(session);
    }

    /// <summary>
    /// Consulta o histórico de sessões da pessoa logada (até 100), opcionalmente filtrando por baralho.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<StudySessionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudySessionResponseDto>>> GetHistory(
        [FromQuery] Guid? deckId,
        [FromQuery] int limit = 20)
    {
        var history = await _studyService.GetHistoryAsync(User.GetUserId(), deckId, limit);
        return Ok(history);
    }
}
