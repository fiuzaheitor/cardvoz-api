using cardvoz_api.Data;
using cardvoz_api.Models;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Services;

public enum SendConfirmationResult
{
    Sent,
    AlreadyConfirmed,
    /// <summary>Pediu de novo em menos de 1 minuto: nada foi enviado.</summary>
    TooSoon,
    NotConfigured,
    NotFound
}

/// <summary>
/// Confirmação de e-mail por link (código de uso único, 24 horas, só o hash no banco).
/// Confirmar não é obrigatório para usar o CardVoz: serve para juntar com segurança
/// a conta com senha e a do Google do mesmo e-mail.
/// </summary>
public class EmailConfirmationService
{
    private static readonly TimeSpan Validity = TimeSpan.FromHours(24);
    private static readonly TimeSpan MinInterval = TimeSpan.FromMinutes(1);

    private readonly CardVozDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly EmailSender _email;
    private readonly ILogger<EmailConfirmationService> _logger;

    public EmailConfirmationService(
        CardVozDbContext context, ITokenService tokenService, EmailSender email, ILogger<EmailConfirmationService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _email = email;
        _logger = logger;
    }

    public async Task<SendConfirmationResult> SendAsync(Guid userId)
    {
        if (!_email.IsConfigured)
            return SendConfirmationResult.NotConfigured;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return SendConfirmationResult.NotFound;
        if (user.EmailConfirmedAt != null)
            return SendConfirmationResult.AlreadyConfirmed;

        var now = DateTime.UtcNow;
        var pending = await _context.EmailConfirmationTokens
            .Where(t => t.UserId == userId && t.UsedAt == null && t.ExpiresAt > now)
            .ToListAsync();
        if (pending.Any(t => now - t.CreatedAt < MinInterval))
            return SendConfirmationResult.TooSoon;
        // Só o link mais novo vale
        foreach (var old in pending)
            old.UsedAt = now;

        var (token, hash) = _tokenService.CreateRefreshToken();
        _context.EmailConfirmationTokens.Add(new EmailConfirmationToken
        {
            UserId = userId,
            TokenHash = hash,
            CreatedAt = now,
            ExpiresAt = now + Validity
        });
        await _context.SaveChangesAsync();

        // O código vai depois do #: o navegador não o envia a nenhum servidor
        var link = $"{_email.SiteUrl}/confirmar-email/#codigo={token}";
        await _email.SendAsync(user.Email, "Confirme o seu e-mail no CardVoz",
            $"Olá, {user.Name}.\n\n" +
            "Para confirmar que este e-mail é seu, abra o link abaixo. Ele vale por 24 horas.\n\n" +
            $"{link}\n\n" +
            "Confirmar não é obrigatório para usar o CardVoz: serve para você também poder " +
            "entrar com o Google usando este e-mail, sem criar outra conta.\n\n" +
            "Se você não criou uma conta no CardVoz, ignore este e-mail.\n\n" +
            "Equipe CardVoz");
        return SendConfirmationResult.Sent;
    }

    /// <summary>Confirma com o código do link. false = código inválido, usado ou vencido.</summary>
    public async Task<bool> ConfirmAsync(string token)
    {
        var hash = _tokenService.HashRefreshToken(token.Trim());
        var now = DateTime.UtcNow;
        var confirmation = await _context.EmailConfirmationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (confirmation?.User == null || confirmation.UsedAt != null || confirmation.ExpiresAt <= now)
            return false;

        confirmation.UsedAt = now;
        confirmation.User.EmailConfirmedAt ??= now;
        await _context.SaveChangesAsync();
        _logger.LogInformation("E-mail confirmado: conta {UserId}", confirmation.UserId);
        return true;
    }
}
