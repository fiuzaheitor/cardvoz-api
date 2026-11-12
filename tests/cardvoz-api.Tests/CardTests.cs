using cardvoz_api.Models;
using Xunit;

namespace cardvoz_api.Tests;

/// <summary>Revisão espaçada (caixa de Leitner): a mesma tabela do site (src/lib/studyStats.ts).</summary>
public class CardTests
{
    private static readonly DateTime T0 = new(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Acertei_sobe_um_nivel_e_afasta_a_revisao_ate_3_meses()
    {
        var card = new Card();
        var dias = new[] { 1, 3, 7, 14, 30, 90, 90 };
        var quando = T0;
        for (var i = 0; i < dias.Length; i++)
        {
            card.RecordAnswer(true, quando);
            Assert.Equal(Math.Min(i + 1, 6), card.ReviewLevel);
            Assert.Equal(quando.AddDays(dias[i]), card.NextReviewAt);
            quando = card.NextReviewAt!.Value;
        }
        Assert.Equal(7, card.CorrectCount);
    }

    [Fact]
    public void Errei_volta_para_revisar_na_hora()
    {
        var card = new Card();
        card.RecordAnswer(true, T0);
        card.RecordAnswer(true, T0.AddDays(1));
        card.RecordAnswer(false, T0.AddDays(4));
        Assert.Equal(0, card.ReviewLevel);
        Assert.Equal(T0.AddDays(4), card.NextReviewAt);
        Assert.False(card.LastWasCorrect);
        Assert.Equal(1, card.IncorrectCount);
    }

    [Fact]
    public void Resposta_antiga_soma_mas_nao_troca_a_ultima_nem_o_nivel()
    {
        var card = new Card();
        card.RecordAnswer(true, T0);
        card.RecordAnswer(false, T0.AddDays(-5));
        Assert.Equal(1, card.ReviewLevel);
        Assert.True(card.LastWasCorrect);
        Assert.Equal(1, card.IncorrectCount);
    }

    [Fact]
    public void Historico_do_aparelho_sem_nivel_e_deduzido()
    {
        var acertou = new Card();
        acertou.SetHistory(2, 1, T0, true);
        Assert.Equal(1, acertou.ReviewLevel);
        Assert.Equal(T0.AddDays(1), acertou.NextReviewAt);

        var errou = new Card();
        errou.SetHistory(0, 1, T0, false, reviewLevel: 4);
        Assert.Equal(0, errou.ReviewLevel);

        var incompleto = new Card();
        incompleto.SetHistory(3, 0, null, true);
        Assert.Null(incompleto.LastAnsweredAt);
        Assert.Null(incompleto.NextReviewAt);
        Assert.Equal(3, incompleto.CorrectCount);
    }
}
