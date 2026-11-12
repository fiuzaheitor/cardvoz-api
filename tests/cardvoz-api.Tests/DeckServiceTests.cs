using cardvoz_api.DTOs.Decks;
using cardvoz_api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace cardvoz_api.Tests;

public class DeckServiceTests
{
    private static ImportDeckDto Import(Guid? id = null, string title = "Inglês") => new()
    {
        Id = id,
        Title = title,
        Category = "Geral",
        QuestionLanguage = "en",
        Cards = { new ImportCardDto { Question = "dog", Answer = "cachorro" }, new ImportCardDto { Question = "cat", Answer = "gato" } }
    };

    [Fact]
    public async Task Importar_cria_baralho_com_fichas_em_ordem_e_idioma()
    {
        using var db = TestDb.Create();
        var user = TestDb.AddUser(db);
        var (outcome, deck) = await new DeckService(db, NullLogger<DeckService>.Instance).ImportAsync(user.Id, Import());

        Assert.Equal(CreateOutcome.Created, outcome);
        Assert.Equal(new[] { "dog", "cat" }, deck!.Cards.Select(c => c.Question));
        Assert.Equal("en", deck.QuestionLanguage);
        Assert.Null(deck.AnswerLanguage);
    }

    [Fact]
    public async Task Mesmo_pedido_repetido_nao_duplica_e_id_de_outra_pessoa_e_conflito()
    {
        using var db = TestDb.Create();
        var ana = TestDb.AddUser(db);
        var bia = TestDb.AddUser(db, "bia@exemplo.com");
        var service = new DeckService(db, NullLogger<DeckService>.Instance);
        var id = Guid.NewGuid();

        await service.ImportAsync(ana.Id, Import(id));
        var (repetido, deck) = await service.ImportAsync(ana.Id, Import(id));
        var (outra, _) = await service.ImportAsync(bia.Id, Import(id));

        Assert.Equal(CreateOutcome.Existing, repetido);
        Assert.Equal(id, deck!.Id);
        Assert.Equal(CreateOutcome.Conflict, outra);
        Assert.Single(db.Decks);
        Assert.Equal(2, db.Cards.Count());
    }

    [Fact]
    public async Task Slug_de_endereco_fixo_e_trocado()
    {
        using var db = TestDb.Create();
        var user = TestDb.AddUser(db);
        var (_, deck) = await new DeckService(db, NullLogger<DeckService>.Instance).ImportAsync(user.Id, Import(title: "Changes"));
        Assert.Equal("changes-1", deck!.Slug);
    }

    [Fact]
    public async Task Edicao_com_idioma_vazio_volta_para_portugues_e_nulo_nao_muda()
    {
        using var db = TestDb.Create();
        var user = TestDb.AddUser(db);
        var service = new DeckService(db, NullLogger<DeckService>.Instance);
        var (_, deck) = await service.ImportAsync(user.Id, Import());

        var semMudar = await service.UpdateAsync(user.Id, deck!.Id, new UpdateDeckDto { AnswerLanguage = "es" });
        Assert.Equal("en", semMudar!.QuestionLanguage);
        Assert.Equal("es", semMudar.AnswerLanguage);

        var portugues = await service.UpdateAsync(user.Id, deck.Id, new UpdateDeckDto { QuestionLanguage = "" });
        Assert.Null(portugues!.QuestionLanguage);
    }
}
