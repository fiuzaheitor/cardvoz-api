using cardvoz_api.Auth;
using cardvoz_api.DTOs.Cards;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cardvoz_api.Controllers;

/// <summary>
/// Fichas dos baralhos da pessoa logada. Fichas de outras pessoas respondem 404.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly ILogger<CardsController> _logger;

    public CardsController(ICardService cardService, ILogger<CardsController> logger)
    {
        _cardService = cardService;
        _logger = logger;
    }

    /// <summary>
    /// Lista cartões de um determinado baralho.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CardResponseDto>>> GetByDeck([FromQuery] Guid? deckId)
    {
        if (!deckId.HasValue)
        {
            return BadRequest(new { message = "O parâmetro 'deckId' é obrigatório para listar cartões." });
        }

        var cards = await _cardService.GetByDeckIdAsync(User.GetUserId(), deckId.Value);
        if (cards == null)
        {
            return NotFound(new { message = $"Baralho com ID {deckId} não foi encontrado." });
        }

        return Ok(cards);
    }

    /// <summary>
    /// Obtém os dados de um cartão específico por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponseDto>> GetById(Guid id)
    {
        var card = await _cardService.GetByIdAsync(User.GetUserId(), id);
        if (card == null)
        {
            return NotFound(new { message = $"Cartão com ID {id} não foi encontrado." });
        }

        return Ok(card);
    }

    /// <summary>
    /// Cadastra um novo cartão de estudo em um baralho.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CardResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponseDto>> Create([FromBody] CreateCardDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _cardService.CreateAsync(User.GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza as informações de um cartão existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CardResponseDto>> Update(Guid id, [FromBody] UpdateCardDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _cardService.UpdateAsync(User.GetUserId(), id, dto);
        if (updated == null)
        {
            return NotFound(new { message = $"Cartão com ID {id} não foi encontrado." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Exclui um cartão de estudo.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _cardService.DeleteAsync(User.GetUserId(), id);
        if (!deleted)
        {
            return NotFound(new { message = $"Cartão com ID {id} não foi encontrado." });
        }

        return NoContent();
    }
}
