using System.Text;

namespace cardvoz_api.Auth;

/// <summary>
/// Configuração dos tokens de login, lida das variáveis de ambiente.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>Token de acesso curto: se vazar, perde a validade rápido.</summary>
    public const int AccessTokenMinutes = 15;

    /// <summary>Token de renovação: mantém a pessoa logada sem digitar a senha de novo.</summary>
    public const int RefreshTokenDays = 30;

    private const int MinSigningKeyBytes = 32;

    public required byte[] SigningKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }

    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        var key = configuration["JWT_SIGNING_KEY"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < MinSigningKeyBytes)
        {
            throw new InvalidOperationException(
                $"Defina a variável JWT_SIGNING_KEY com pelo menos {MinSigningKeyBytes} caracteres aleatórios " +
                "(ex.: gere com: openssl rand -base64 48).");
        }

        return new JwtSettings
        {
            SigningKey = Encoding.UTF8.GetBytes(key),
            Issuer = configuration["JWT_ISSUER"] ?? "cardvoz-api",
            Audience = configuration["JWT_AUDIENCE"] ?? "cardvoz",
        };
    }
}
