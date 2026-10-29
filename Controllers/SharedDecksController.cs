using cardvoz_api.Auth;
using cardvoz_api.DTOs.Decks;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cardvoz_api.Controllers;

/// <summary>
/// Baralhos compartilhados, criados pelo painel de admin. Qualquer pessoa logada
/// vê a lista e adiciona uma cópia aos próprios baralhos para estudar e editar.
/// </summary>
[ApiController]
[Authorize]
[Route("api/shared-decks")]
[Produces("application/json")]
public class SharedDecksController : ControllerBase
{
    private readonly IDeckService _deckService;

    public SharedDecksController(IDeckService deckService)
    {
        _deckService = deckService;
    }

    /// <summary>Lista os baralhos compartilhados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeckResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DeckResponseDto>>> GetAll() => Ok(await _deckService.GetSharedAsync());

    /// <summary>Adiciona uma cópia do baralho, com as fichas, aos baralhos da pessoa logada.</summary>
    [HttpPost("{id:guid}/copy")]
    [ProducesResponseType(typeof(DeckResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeckResponseDto>> Copy(Guid id)
    {
        var deck = await _deckService.CopySharedAsync(User.GetUserId(), id);
        return deck == null
            ? NotFound(new { message = "Baralho compartilhado não encontrado." })
            : StatusCode(StatusCodes.Status201Created, deck);
    }
}
