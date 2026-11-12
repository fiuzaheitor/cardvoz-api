using System.Net;
using System.Net.Mail;
using cardvoz_api.Auth;

namespace cardvoz_api.Services;

/// <summary>
/// Envia e-mails de texto por SMTP (configuração em EmailSettings). Usado por
/// "Esqueci minha senha" e pela confirmação de e-mail.
/// </summary>
public class EmailSender
{
    private readonly EmailSettings _settings;

    public EmailSender(EmailSettings settings)
    {
        _settings = settings;
    }

    public bool IsConfigured => _settings.IsConfigured;

    /// <summary>Endereço do site, sem barra no final, para montar os links.</summary>
    public string SiteUrl => _settings.SiteUrl ?? "";

    public virtual async Task SendAsync(string to, string subject, string body)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.From!),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(to);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrEmpty(_settings.User) ? null : new NetworkCredential(_settings.User, _settings.Password)
        };
        await client.SendMailAsync(message);
    }
}
