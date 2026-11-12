using System.Security.Cryptography;
using cardvoz_api.Auth;
using cardvoz_api.Models;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace cardvoz_api.Tests;

public class EmailConfirmationTests
{
    private static readonly TokenService Tokens = new(new JwtSettings
    {
        SigningKey = RandomNumberGenerator.GetBytes(48),
        Issuer = "cardvoz-api",
        Audience = "cardvoz"
    });

    /// <summary>Guarda os e-mails em vez de enviar.</summary>
    private sealed class FakeSender : EmailSender
    {
        public List<(string To, string Body)> Sent { get; } = new();
        public FakeSender() : base(new EmailSettings { Host = "smtp", From = "x@y.com", SiteUrl = "https://cardvoz.test" }) { }
        public override Task SendAsync(string to, string subject, string body)
        {
            Sent.Add((to, body));
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Link_confirma_o_email_uma_vez_so()
    {
        using var db = TestDb.Create();
        var user = TestDb.AddUser(db);
        var sender = new FakeSender();
        var service = new EmailConfirmationService(db, Tokens, sender, NullLogger<EmailConfirmationService>.Instance);

        Assert.Equal(SendConfirmationResult.Sent, await service.SendAsync(user.Id));
        Assert.Equal(SendConfirmationResult.TooSoon, await service.SendAsync(user.Id));
        var code = sender.Sent.Single().Body.Split("#codigo=")[1].Split('\n')[0];

        Assert.True(await service.ConfirmAsync(code));
        Assert.NotNull(user.EmailConfirmedAt);
        Assert.False(await service.ConfirmAsync(code));
        Assert.Equal(SendConfirmationResult.AlreadyConfirmed, await service.SendAsync(user.Id));
    }

    [Fact]
    public async Task Google_junta_com_a_conta_de_senha_so_se_o_email_estiver_confirmado()
    {
        using var db = TestDb.Create();
        var user = TestDb.AddUser(db);
        user.PasswordHash = "hash";
        db.SaveChanges();
        var auth = new AuthService(db, new PasswordHasher<User>(), Tokens, NullLogger<AuthService>.Instance);
        var google = new GoogleIdentity("sub-123", "ana@exemplo.com", true, "Ana");

        var semConfirmar = await auth.LoginWithGoogleAsync(google);
        Assert.Equal(AuthFailure.GoogleEmailHasPasswordAccount, semConfirmar.Failure);

        user.EmailConfirmedAt = DateTime.UtcNow;
        db.SaveChanges();
        var confirmado = await auth.LoginWithGoogleAsync(google);
        Assert.NotNull(confirmado.Response);
        Assert.Equal(user.Id, confirmado.Response!.User.Id);
        Assert.Equal("sub-123", user.GoogleSubject);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task Conta_nova_pelo_Google_ja_nasce_confirmada()
    {
        using var db = TestDb.Create();
        var auth = new AuthService(db, new PasswordHasher<User>(), Tokens, NullLogger<AuthService>.Instance);
        var result = await auth.LoginWithGoogleAsync(new GoogleIdentity("sub-9", "bia@exemplo.com", true, "Bia"));
        Assert.True(result.Response!.User.EmailConfirmed);
    }
}
