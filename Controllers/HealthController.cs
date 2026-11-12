using cardvoz_api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Controllers;

/// <summary>
/// Confere se a API e o banco respondem, sem login. A Action "Manter ativo" chama
/// esta rota a cada 10 minutos: mantém o Render acordado (sem a espera de até 1
/// minuto na primeira chamada) e a consulta ao banco conta como uso, para o
/// Supabase do plano gratuito não pausar o projeto por falta de atividade.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly CardVozDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(CardVozDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            // Uma leitura de verdade numa tabela (e não só abrir a conexão)
            await _context.Decks.AsNoTracking().AnyAsync(cancellationToken);
            return Ok(new { status = "ok" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "O banco não respondeu na conferência de saúde.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "banco indisponível" });
        }
    }
}
