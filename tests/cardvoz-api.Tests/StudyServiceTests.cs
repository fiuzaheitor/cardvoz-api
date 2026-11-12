using cardvoz_api.DTOs.Decks;
using cardvoz_api.DTOs.StudySessions;
using cardvoz_api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace cardvoz_api.Tests;

public class StudyServiceTests
{
    [Fact]
    public async Task Sessao_completa_soma_nas_fichas_ignora_fichas_de_fora_e_nao_repete()
    {
        using var db = TestDb.Create();
        var ana = TestDb.AddUser(db);
        var bia = TestDb.AddUser(db, "bia@exemplo.com");
        var deck = TestDb.AddDeck(db, ana, "a", "b");
        var alheio = TestDb.AddDeck(db, bia, "x");
        var service = new StudyService(db, NullLogger<StudyService>.Instance);
        var quando = DateTime.UtcNow.AddMinutes(-5);
        var dto = new CompleteSessionDto
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            StartedAt = quando,
            CompletedAt = quando.AddMinutes(1),
            Answers =
            {
                new CompleteAnswerDto { CardId = deck.Cards.First().Id, WasCorrect = true, AnsweredAt = quando },
                new CompleteAnswerDto { CardId = deck.Cards.Last().Id, WasCorrect = false, AnsweredAt = quando },
                new CompleteAnswerDto { CardId = alheio.Cards.First().Id, WasCorrect = true, AnsweredAt = quando },
            }
        };

        var (criada, sessao) = await service.CompleteSessionAsync(ana.Id, dto);
        var (repetida, _) = await service.CompleteSessionAsync(ana.Id, dto);
        var (deOutra, _) = await service.CompleteSessionAsync(bia.Id, dto);

        Assert.Equal(CreateOutcome.Created, criada);
        Assert.Equal(2, sessao!.TotalCards);
        Assert.Equal(CreateOutcome.Existing, repetida);
        Assert.Equal(CreateOutcome.Conflict, deOutra);
        Assert.Equal(1, deck.Cards.First().CorrectCount);
        Assert.Equal(1, deck.Cards.First().ReviewLevel);
        Assert.Equal(1, deck.Cards.Last().IncorrectCount);
        Assert.Equal(0, alheio.Cards.First().CorrectCount);
        Assert.NotNull(deck.LastStudiedAt);
    }

    [Fact]
    public async Task Baralho_de_outra_pessoa_nao_e_encontrado()
    {
        using var db = TestDb.Create();
        var ana = TestDb.AddUser(db);
        var bia = TestDb.AddUser(db, "bia@exemplo.com");
        var deck = TestDb.AddDeck(db, bia, "x");
        var (outcome, _) = await new StudyService(db, NullLogger<StudyService>.Instance)
            .CompleteSessionAsync(ana.Id, new CompleteSessionDto { DeckId = deck.Id, StartedAt = DateTime.UtcNow, CompletedAt = DateTime.UtcNow });
        Assert.Equal(CreateOutcome.NotFound, outcome);
    }
}
