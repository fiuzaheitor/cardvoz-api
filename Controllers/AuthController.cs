using cardvoz_api.Auth;
using cardvoz_api.DTOs.Auth;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace cardvoz_api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Cria uma conta e já devolve os tokens de acesso e de renovação.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (result.Response == null)
        {
            return Conflict(new { message = "Já existe uma conta com este e-mail." });
        }

        return CreatedAtAction(nameof(Me), null, result.Response);
    }

    /// <summary>
    /// Entra com e-mail e senha. Após 5 senhas erradas seguidas, a conta fica bloqueada por 15 minutos.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result.Failure == AuthFailure.LockedOut && result.LockoutEnd.HasValue)
        {
            var seconds = Math.Max(1, (int)Math.Ceiling((result.LockoutEnd.Value - DateTime.UtcNow).TotalSeconds));
            Response.Headers.RetryAfter = seconds.ToString();
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = $"Muitas tentativas de login. Tente de novo em {Math.Ceiling(seconds / 60.0)} minuto(s).",
            });
        }

        if (result.Response == null)
        {
            // Mesma mensagem para e-mail inexistente e senha errada
            return Unauthorized(new { message = "E-mail ou senha incorretos." });
        }

        return Ok(result.Response);
    }

    /// <summary>
    /// Troca um token de renovação válido por um novo par de tokens. O token usado deixa de valer.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshAsync(dto.RefreshToken);
        if (result.Response == null)
        {
            return Unauthorized(new { message = "Sessão expirada. Entre novamente." });
        }

        return Ok(result.Response);
    }

    /// <summary>
    /// Encerra a sessão revogando o token de renovação. Sempre responde 204.
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        await _authService.LogoutAsync(dto.RefreshToken);
        return NoContent();
    }

    /// <summary>
    /// Dados da pessoa logada.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponseDto>> Me()
    {
        var profile = await _authService.GetProfileAsync(User.GetUserId());
        if (profile == null)
        {
            // Conta excluída com token ainda válido
            return Unauthorized(new { message = "Sessão inválida. Entre novamente." });
        }

        return Ok(profile);
    }
}
