namespace cardvoz_api.Auth;

/// <summary>
/// Envio de e-mail por SMTP (serve para Gmail, Brevo, Resend e parecidos), lido das
/// variáveis de ambiente. Sem ele a API sobe normalmente, mas "Esqueci minha senha"
/// responde 503.
/// </summary>
public sealed class EmailSettings
{
    public string? Host { get; init; }
    public int Port { get; init; } = 587;
    public string? User { get; init; }
    public string? Password { get; init; }
    /// <summary>Remetente, ex.: "CardVoz &lt;nao-responda@exemplo.com&gt;".</summary>
    public string? From { get; init; }
    /// <summary>Endereço do site, para montar o link (ex.: https://cardvoz.onrender.com).</summary>
    public string? SiteUrl { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(From) && !string.IsNullOrWhiteSpace(SiteUrl);

    public static EmailSettings FromConfiguration(IConfiguration configuration) => new()
    {
        Host = configuration["SMTP_HOST"]?.Trim(),
        Port = int.TryParse(configuration["SMTP_PORT"], out var port) ? port : 587,
        User = configuration["SMTP_USER"]?.Trim(),
        Password = configuration["SMTP_PASSWORD"],
        From = configuration["SMTP_FROM"]?.Trim(),
        SiteUrl = configuration["SITE_URL"]?.Trim().TrimEnd('/'),
    };
}
