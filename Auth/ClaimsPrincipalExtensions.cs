using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace cardvoz_api.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// ID da pessoa logada, lido do claim "sub" do token de acesso.
    /// Só deve ser chamado em rotas protegidas por [Authorize].
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("Token de acesso sem identificador de usuário.");
    }
}
