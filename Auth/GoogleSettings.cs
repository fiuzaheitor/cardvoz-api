namespace cardvoz_api.Auth;

/// <summary>
/// Configuração do login com Google, lida das variáveis de ambiente.
/// Sem ela a API sobe normalmente, mas a rota /api/auth/google responde 503.
/// </summary>
public sealed class GoogleSettings
{
    /// <summary>Página do site que recebe a volta do Google (precisa estar cadastrada no Google Cloud).</summary>
    public const string RedirectPath = "/entrar/";

    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }

    /// <summary>Origens do site (as mesmas do CORS) para onde o Google pode devolver a pessoa.</summary>
    public required string[] AllowedOrigins { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);

    /// <summary>
    /// Só aceita voltar para a página de login do próprio site. O Google também
    /// confere o endereço, mas checar aqui evita usar a API com endereços de terceiros.
    /// </summary>
    public bool IsAllowedRedirectUri(string redirectUri)
    {
        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri)) return false;
        if (uri.AbsolutePath != RedirectPath || uri.Query.Length > 0 || uri.Fragment.Length > 0) return false;

        var origin = uri.GetLeftPart(UriPartial.Authority);
        return AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }

    public static GoogleSettings FromConfiguration(IConfiguration configuration, string[] allowedOrigins) => new()
    {
        ClientId = configuration["GOOGLE_CLIENT_ID"]?.Trim(),
        ClientSecret = configuration["GOOGLE_CLIENT_SECRET"]?.Trim(),
        AllowedOrigins = allowedOrigins.Select(o => o.TrimEnd('/')).ToArray(),
    };
}
