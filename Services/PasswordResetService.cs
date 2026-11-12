using System.Net;
using System.Net.Mail;
using cardvoz_api.Auth;
using cardvoz_api.Data;
using cardvoz_api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Services;

/// <summary>
/// "Esqueci minha senha": manda por e-mail um link com um código de uso único
/// (1 hora) e troca a senha com ele. Também serve para quem entra só com o Google
/// criar uma senha: o link prova que a pessoa recebe os e-mails da conta.
/// </summary>
public class PasswordResetService
{
    private static readonly TimeSpan Validity = TimeSpan.FromHours(1);
    /// <summary>Um e-mail por minuto por conta: pedir várias vezes não lota a caixa de ninguém.</summary>
    private static readonly TimeSpan MinInterval = TimeSpan.FromMinutes(1);

    private readonly CardVozDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly EmailSettings _email;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        CardVozDbContext context,
        ITokenService tokenService,
        IPasswordHasher<User> passwordHasher,
        EmailSettings email,
        ILogger<PasswordResetService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _email = email;
        _logger = logger;
    }

    public bool IsConfigured => _email.IsConfigured;

    /// <summary>
    /// Cria o código e manda o e-mail, se a conta existir. Quem chama responde sempre
    /// a mesma coisa: a rota não pode servir para descobrir quais e-mails têm conta.
    /// </summary>
    public async Task RequestAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalized);
        if (user == null)
            return;

        var now = DateTime.UtcNow;
        var pending = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.UsedAt == null && t.ExpiresAt > now)
            .ToListAsync();
        if (pending.Any(t => now - t.CreatedAt < MinInterval))
            return;

        // Só o link mais novo vale
        foreach (var old in pending)
            old.UsedAt = now;

        var (token, hash) = _tokenService.CreateRefreshToken();
        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hash,
            CreatedAt = now,
            ExpiresAt = now + Validity
        });
        await _context.SaveChangesAsync();

        // O código vai depois do #: o navegador não o envia a nenhum servidor nem o
        // deixa em logs de acesso
        var link = $"{_email.SiteUrl}/redefinir-senha/#codigo={token}";
        try
        {
            await SendAsync(user.Email, "Criar uma nova senha no CardVoz",
                $"Olá, {user.Name}.\n\n" +
                "Recebemos um pedido para criar uma nova senha na sua conta do CardVoz. " +
                "Para continuar, abra o link abaixo. Ele vale por 1 hora e funciona uma vez só.\n\n" +
                $"{link}\n\n" +
                "Se não foi você, ignore este e-mail: a sua senha continua a mesma.\n\n" +
                "Equipe CardVoz");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Não foi possível enviar o e-mail de nova senha para a conta {UserId}.", user.Id);
        }
    }

    /// <summary>Troca a senha com um código válido e encerra as sessões abertas da conta.</summary>
    public async Task<bool> ResetAsync(string token, string newPassword)
    {
        var hash = _tokenService.HashRefreshToken(token.Trim());
        var now = DateTime.UtcNow;
        var reset = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (reset?.User == null || reset.UsedAt != null || reset.ExpiresAt <= now)
            return false;

        var user = reset.User;
        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = now;
        reset.UsedAt = now;

        // Quem pegou a senha antiga perde o acesso
        await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now));
        await _context.SaveChangesAsync();

        _logger.LogInformation("Senha redefinida pelo link de e-mail: conta {UserId}", user.Id);
        return true;
    }

    private async Task SendAsync(string to, string subject, string body)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_email.From!),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(to);

        using var client = new SmtpClient(_email.Host, _email.Port)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrEmpty(_email.User) ? null : new NetworkCredential(_email.User, _email.Password)
        };
        await client.SendMailAsync(message);
    }
}
