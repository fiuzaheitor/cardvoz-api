namespace cardvoz_api.Services;

/// <summary>
/// Dados da conta Google confirmados pelo id_token.
/// </summary>
public sealed record GoogleIdentity(string Subject, string Email, bool EmailVerified, string? Name);

public interface IGoogleAuthService
{
    /// <summary>
    /// Troca o código de autorização pelo id_token e o valida.
    /// Devolve null se o código for inválido, vencido ou já usado.
    /// </summary>
    Task<GoogleIdentity?> ExchangeCodeAsync(
        string code, string codeVerifier, string redirectUri, CancellationToken cancellationToken = default);
}
