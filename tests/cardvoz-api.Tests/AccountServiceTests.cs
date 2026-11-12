using cardvoz_api.DTOs.Account;
using cardvoz_api.Models;
using cardvoz_api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace cardvoz_api.Tests;

public class AccountServiceTests
{
    private static readonly PasswordHasher<User> Hasher = new();

    private static AccountService Service(cardvoz_api.Data.CardVozDbContext db) =>
        new(db, Hasher, NullLogger<AccountService>.Instance);

    [Fact]
    public async Task Excluir_confere_email_e_senha_e_apaga_os_baralhos()
    {
        using var db = TestDb.Create();
        var user = TestDb.AddUser(db);
        user.PasswordHash = Hasher.HashPassword(user, "senha-forte-123");
        db.SaveChanges();
        TestDb.AddDeck(db, user, "a");

        Assert.Equal(DeleteAccountResult.WrongEmail, await Service(db).DeleteAsync(user.Id, new DeleteAccountDto { ConfirmEmail = "outra@exemplo.com", Password = "senha-forte-123" }));
        Assert.Equal(DeleteAccountResult.WrongPassword, await Service(db).DeleteAsync(user.Id, new DeleteAccountDto { ConfirmEmail = "ANA@exemplo.com", Password = "errada" }));
        Assert.Equal(DeleteAccountResult.Deleted, await Service(db).DeleteAsync(user.Id, new DeleteAccountDto { ConfirmEmail = "ANA@exemplo.com", Password = "senha-forte-123" }));
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task Unica_pessoa_admin_nao_exclui_a_conta()
    {
        using var db = TestDb.Create();
        var admin = TestDb.AddUser(db, isAdmin: true);
        var result = await Service(db).DeleteAsync(admin.Id, new DeleteAccountDto { ConfirmEmail = admin.Email });
        Assert.Equal(DeleteAccountResult.LastAdmin, result);
    }

    [Fact]
    public async Task Exportar_nao_traz_senha_nem_tokens()
    {
        using var db = TestDb.Create();
        var user = TestDb.AddUser(db);
        user.PasswordHash = "hash-secreto";
        db.SaveChanges();
        TestDb.AddDeck(db, user, "a");
        var json = System.Text.Json.JsonSerializer.Serialize(await Service(db).ExportAsync(user.Id));
        Assert.DoesNotContain("hash-secreto", json);
        Assert.Contains("\"question\":\"a\"", json);
    }
}
