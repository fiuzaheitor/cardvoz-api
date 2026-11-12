using cardvoz_api.Auth;
using cardvoz_api.DTOs.Account;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace cardvoz_api.Controllers;

/// <summary>
/// Dados da própria conta (LGPD): baixar tudo e excluir a conta.
/// </summary>
[ApiController]
[Authorize]
[Route("api/account")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>Tudo o que a conta guarda: dados da conta, baralhos, fichas e histórico de estudo.</summary>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Export()
    {
        var data = await _accountService.ExportAsync(User.GetUserId());
        return data == null ? NotFound(new { message = "Conta não encontrada." }) : Ok(data);
    }

    /// <summary>
    /// Exclui a conta com tudo o que ela guarda. Pede o e-mail e, em conta com senha,
    /// a senha. Não dá para desfazer.
    /// </summary>
    [HttpPost("delete")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromBody] DeleteAccountDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return await _accountService.DeleteAsync(User.GetUserId(), dto) switch
        {
            DeleteAccountResult.Deleted => NoContent(),
            DeleteAccountResult.NotFound => NotFound(new { message = "Conta não encontrada." }),
            DeleteAccountResult.WrongEmail => FieldError(nameof(dto.ConfirmEmail), "O e-mail digitado não é o desta conta."),
            DeleteAccountResult.WrongPassword => FieldError(nameof(dto.Password), "Senha incorreta."),
            _ => Conflict(new { message = "Você é a única pessoa admin. Dê o acesso de admin a outra conta antes de excluir a sua." })
        };
    }

    private BadRequestObjectResult FieldError(string field, string message)
    {
        ModelState.AddModelError(field, message);
        return BadRequest(ModelState);
    }
}
