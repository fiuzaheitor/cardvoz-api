using cardvoz_api.Auth;
using cardvoz_api.DTOs.Cards;
using cardvoz_api.DTOs.Decks;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cardvoz_api.Controllers;

/// <summary>
/// Baralhos da pessoa logada. Baralhos de outras pessoas respondem 404.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public class DecksController : ControllerBase
{
    private readonly IDeckService _deckService;
    private readonly ICardService _cardService;
    private readonly ILogger<DecksController> _logger;

    public DecksController(
        IDeckService deckService,
        ICardService cardService,
        ILogger<DecksController> logger)
    {
        _deckService = deckService;
        _cardService = cardService;
        _logger = logger;
    }

    /// <summary>
    /// Lista os baralhos da pessoa logada, com opção de filtro por categoria e busca textual.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeckResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DeckResponseDto>>> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? search)
    {
        var decks = await _deckService.GetAllAsync(User.GetUserId(), category, search);
        return Ok(decks);
    }

    /// <summary>
    /// Retorna as categorias usadas nos baralhos da pessoa logada.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _deckService.GetCategoriesAsync(User.GetUserId());
        return Ok(categories);
    }

    /// <summary>
    /// Obtém detalhes completos de um baralho e seus cartões pelo ID (UUID) ou slug.
    /// </summary>
    [HttpGet("{idOrSlug}")]
    [ProducesResponseType(typeof(DeckDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeckDetailResponseDto>> GetByIdOrSlug(string idOrSlug)
    {
        var deck = await _deckService.GetByIdOrSlugAsync(User.GetUserId(), idOrSlug);
        if (deck == null)
        {
            return NotFound(new { message = $"Baralho '{idOrSlug}' não foi encontrado." });
        }

        return Ok(deck);
    }

    /// <summary>
    /// Retorna os cartões pertencentes a um baralho específico.
    /// </summary>
    [HttpGet("{deckId:guid}/cards")]
    [ProducesResponseType(typeof(IEnumerable<CardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CardResponseDto>>> GetCardsByDeck(Guid deckId)
    {
        var cards = await _cardService.GetByDeckIdAsync(User.GetUserId(), deckId);
        if (cards == null)
        {
            return NotFound(new { message = $"Baralho com ID {deckId} não foi encontrado." });
        }

        return Ok(cards);
    }

    /// <summary>
    /// Cria um novo baralho.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DeckResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeckResponseDto>> Create([FromBody] CreateDeckDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var deck = await _deckService.CreateAsync(User.GetUserId(), dto);
        return CreatedAtAction(nameof(GetByIdOrSlug), new { idOrSlug = deck.Slug }, deck);
    }

    /// <summary>
    /// Atualiza as informações de um baralho existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DeckResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeckResponseDto>> Update(Guid id, [FromBody] UpdateDeckDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _deckService.UpdateAsync(User.GetUserId(), id, dto);
        if (updated == null)
        {
            return NotFound(new { message = $"Baralho com ID {id} não foi encontrado." });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Remove um baralho e todos os seus cartões associados.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _deckService.DeleteAsync(User.GetUserId(), id);
        if (!deleted)
        {
            return NotFound(new { message = $"Baralho com ID {id} não foi encontrado." });
        }

        return NoContent();
    }
}
