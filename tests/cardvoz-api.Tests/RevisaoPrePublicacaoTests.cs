using System.ComponentModel.DataAnnotations;
using cardvoz_api.DTOs.Decks;
using cardvoz_api.DTOs.StudySessions;
using cardvoz_api.Models;
using cardvoz_api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace cardvoz_api.Tests;

/// <summary>Correções da revisão antes da publicação (nov/2026).</summary>
public class RevisaoPrePublicacaoTests
{
    [Fact]
    public async Task Copia_de_baralho_pronto_devolve_de_onde_veio_no_detalhe()
    {
        // Sem o sourceDeckId, o site não sabia que já tinha a cópia e deixava copiar de novo
        using var db = TestDb.Create();
        var user = TestDb.AddUser(db);
        var shared = new Deck { UserId = null, Title = "Inglês", Slug = "ingles", Category = "Geral" };
        shared.Cards = new List<Card> { new() { DeckId = shared.Id, Question = "dog", Answer = "cão", DisplayOrder = 1 } };
        db.Decks.Add(shared);
        db.SaveChanges();
        var service = new DeckService(db, NullLogger<DeckService>.Instance);

        var copy = await service.CopySharedAsync(user.Id, shared.Id);
        var detail = await service.GetByIdOrSlugAsync(user.Id, copy!.Id.ToString());

        Assert.Equal(shared.Id, detail!.SourceDeckId);
    }

    [Fact]
    public void Historico_com_data_no_futuro_fica_com_a_hora_do_servidor()
    {
        // Relógio do aparelho adiantado em 1 dia
        var card = new Card();
        var amanha = DateTime.UtcNow.AddDays(1);
        card.SetHistory(3, 1, amanha, true, 2, amanha.AddDays(3));

        Assert.True(card.LastAnsweredAt <= DateTime.UtcNow);
        Assert.True(card.NextReviewAt <= card.LastAnsweredAt + Card.ReviewIntervals[2]);

        // Um estudo de agora volta a mexer na revisão
        card.RecordAnswer(false, DateTime.UtcNow);
        Assert.Equal(0, card.ReviewLevel);
        Assert.False(card.LastWasCorrect);
    }

    private static List<ValidationResult> Validar(object dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Sessao_sem_horarios_ou_com_resposta_nula_e_recusada()
    {
        var semHorario = new CompleteSessionDto { DeckId = Guid.NewGuid() };
        Assert.Contains(Validar(semHorario), r => r.MemberNames.Contains(nameof(CompleteSessionDto.StartedAt)));

        var comNula = new CompleteSessionDto
        {
            DeckId = Guid.NewGuid(), StartedAt = DateTime.UtcNow, CompletedAt = DateTime.UtcNow,
            Answers = new List<CompleteAnswerDto> { null! },
        };
        Assert.Contains(Validar(comNula), r => r.MemberNames.Contains(nameof(CompleteSessionDto.Answers)));
    }

    [Fact]
    public void Importacao_com_ficha_nula_e_recusada()
    {
        var dto = new ImportDeckDto { Title = "Bio", Category = "Geral", Cards = new List<ImportCardDto> { null! } };
        Assert.Contains(Validar(dto), r => r.MemberNames.Contains(nameof(ImportDeckDto.Cards)));
    }
}
