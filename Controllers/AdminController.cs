using cardvoz_api.Auth;
using cardvoz_api.DTOs.Admin;
using cardvoz_api.DTOs.Decks;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cardvoz_api.Controllers;

/// <summary>
/// Painel de admin (cardvoz-admin): usuários e baralhos compartilhados. Só admins (403 para os demais).
/// </summary>
[ApiController]
[Authorize]
[AdminOnly]
[Route("api/admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IDeckService _deckService;

    public AdminController(IAdminService adminService, IDeckService deckService)
    {
        _adminService = adminService;
        _deckService = deckService;
    }

    /// <summary>Números gerais do CardVoz.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatsDto>> GetStats() => Ok(await _adminService.GetStatsAsync());

    /// <summary>Todas as contas, das mais novas para as mais antigas.</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetUsers() => Ok(await _adminService.GetUsersAsync());

    /// <summary>Dá ou tira o acesso ao painel. Ninguém muda o próprio acesso.</summary>
    [HttpPut("users/{id:guid}/admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAdmin(Guid id, [FromBody] SetAdminDto dto) =>
        ToResult(await _adminService.SetAdminAsync(User.GetUserId(), id, dto.IsAdmin!.Value),
            "Você não pode mudar o seu próprio acesso de admin.");

    /// <summary>Exclui a conta com todos os baralhos, fichas e sessões dela. Ninguém exclui a própria conta.</summary>
    [HttpDelete("users/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id) =>
        ToResult(await _adminService.DeleteUserAsync(User.GetUserId(), id),
            "Você não pode excluir a sua própria conta pelo painel.");

    /// <summary>Lista os baralhos compartilhados.</summary>
    [HttpGet("shared-decks")]
    [ProducesResponseType(typeof(IEnumerable<DeckResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DeckResponseDto>>> GetSharedDecks() =>
        Ok(await _deckService.GetSharedAsync());

    /// <summary>Um baralho compartilhado com as fichas.</summary>
    [HttpGet("shared-decks/{id:guid}")]
    [ProducesResponseType(typeof(DeckDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeckDetailResponseDto>> GetSharedDeck(Guid id)
    {
        var deck = await _deckService.GetSharedByIdAsync(id);
        return deck == null ? NotFound(new { message = "Baralho compartilhado não encontrado." }) : Ok(deck);
    }

    /// <summary>Cria um baralho compartilhado com as fichas.</summary>
    [HttpPost("shared-decks")]
    [ProducesResponseType(typeof(DeckResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeckResponseDto>> CreateSharedDeck([FromBody] SharedDeckDto dto)
    {
        var deck = await _deckService.CreateSharedAsync(dto);
        return CreatedAtAction(nameof(GetSharedDeck), new { id = deck.Id }, deck);
    }

    /// <summary>Substitui título, descrição e fichas. As cópias já feitas não mudam.</summary>
    [HttpPut("shared-decks/{id:guid}")]
    [ProducesResponseType(typeof(DeckResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeckResponseDto>> UpdateSharedDeck(Guid id, [FromBody] SharedDeckDto dto)
    {
        var deck = await _deckService.UpdateSharedAsync(id, dto);
        return deck == null ? NotFound(new { message = "Baralho compartilhado não encontrado." }) : Ok(deck);
    }

    /// <summary>Exclui o baralho compartilhado. As cópias já feitas continuam com quem copiou.</summary>
    [HttpDelete("shared-decks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSharedDeck(Guid id) =>
        await _deckService.DeleteSharedAsync(id)
            ? NoContent()
            : NotFound(new { message = "Baralho compartilhado não encontrado." });

    private IActionResult ToResult(AdminChange change, string selfMessage) => change switch
    {
        AdminChange.Done => NoContent(),
        AdminChange.Self => BadRequest(new { message = selfMessage }),
        AdminChange.NotAdmin => Forbid(),
        _ => NotFound(new { message = "Usuário não encontrado." }),
    };
}
