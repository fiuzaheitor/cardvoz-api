using cardvoz_api.DTOs.Cards;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace cardvoz_api.Controllers;

[ApiController]
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
    public async Task<ActionResult<IEnumerable<CardResponseDto>>> GetByDeck([FromQuery] int? deckId)
    {
        if (!deckId.HasValue)
        {
            return BadRequest(new { message = "O parâmetro 'deckId' é obrigatório para listar cartões." });
        }

        var cards = await _cardService.GetByDeckIdAsync(deckId.Value);
        return Ok(cards);
    }

    /// <summary>
    /// Obtém os dados de um cartão específico por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponseDto>> GetById(int id)
    {
        var card = await _cardService.GetByIdAsync(id);
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
            var created = await _cardService.CreateAsync(dto);
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
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CardResponseDto>> Update(int id, [FromBody] UpdateCardDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _cardService.UpdateAsync(id, dto);
        if (updated == null)
        {
            return NotFound(new { message = $"Cartão com ID {id} não foi encontrado." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Exclui um cartão de estudo.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _cardService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Cartão com ID {id} não foi encontrado." });
        }

        return NoContent();
    }
}
