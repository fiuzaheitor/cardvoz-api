using cardvoz_api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Auth;

/// <summary>
/// Libera a rota só para admins (403 para os demais). Confere no banco a cada
/// chamada, e não no token: quem perde o admin perde o acesso na hora.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AdminOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Sem login, a política padrão já respondeu 401 antes de chegar aqui
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;

        var db = context.HttpContext.RequestServices.GetRequiredService<CardVozDbContext>();
        var userId = context.HttpContext.User.GetUserId();
        if (!await db.Users.AnyAsync(u => u.Id == userId && u.IsAdmin))
        {
            context.Result = new ForbidResult();
        }
    }
}
