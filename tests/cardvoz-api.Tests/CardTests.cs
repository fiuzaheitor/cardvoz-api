using cardvoz_api.Models;
using Xunit;

namespace cardvoz_api.Tests;

/// <summary>
/// Revisão espaçada no estilo do Anki. Os mesmos casos e números do site
/// (src/lib/studyStats.test.ts): as duas contas precisam dar o mesmo resultado.
/// </summary>
public class CardTests
{
    private static readonly DateTime T0 = new(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc);

    /// <summary>Ficha em revisão: intervalo, facilidade e última resposta em T0.</summary>
    private static Card EmRevisao(int dias, short facilidade = Card.DefaultReviewEase)
    {
        var card = new Card();
        card.SetHistory(1, 0, T0, true, facilidade, T0.AddDays(dias));
        return card;
    }

    [Fact]
    public void Acertei_na_data_multiplica_o_intervalo_pela_facilidade_ate_1_ano()
    {
        var card = new Card();
        var dias = new[] { 1, 3, 8, 20, 50, 125, 313, 365, 365 };
        var quando = T0;
        foreach (var esperado in dias)
        {
            card.RecordAnswer(true, quando);
            Assert.Equal(esperado, card.ReviewIntervalDays);
            Assert.Equal(quando.AddDays(esperado), card.NextReviewAt);
            quando = card.NextReviewAt!.Value;
        }
        Assert.Equal(dias.Length, card.CorrectCount);
        Assert.Equal(Card.DefaultReviewEase, card.ReviewEase);
    }

    [Fact]
    public void Errei_volta_para_revisar_na_hora_e_baixa_a_facilidade_uma_vez()
    {
        var card = EmRevisao(8);
        card.RecordAnswer(false, T0.AddDays(8));
        Assert.Equal(0, card.ReviewIntervalDays);
        Assert.Equal(2300, card.ReviewEase);
        Assert.Equal(T0.AddDays(8), card.NextReviewAt);
        Assert.False(card.LastWasCorrect);

        // Errar de novo antes de acertar não baixa a facilidade outra vez
        card.RecordAnswer(false, T0.AddDays(8).AddMinutes(5));
        Assert.Equal(2300, card.ReviewEase);
        Assert.Equal(2, card.IncorrectCount);

        // Acertou: recomeça em 1 dia e cresce mais devagar (1 × 2,3 = 2; 2 × 2,3 = 5)
        var acerto = T0.AddDays(9);
        card.RecordAnswer(true, acerto);
        Assert.Equal(1, card.ReviewIntervalDays);
        card.RecordAnswer(true, acerto.AddDays(1));
        Assert.Equal(2, card.ReviewIntervalDays);
        card.RecordAnswer(true, acerto.AddDays(3));
        Assert.Equal(5, card.ReviewIntervalDays);
    }

    [Fact]
    public void A_facilidade_nao_passa_do_minimo()
    {
        var card = new Card();
        var quando = T0;
        for (var i = 0; i < 8; i++)
        {
            card.RecordAnswer(true, quando);
            quando = card.NextReviewAt!.Value;
            card.RecordAnswer(false, quando);
        }
        Assert.Equal(Card.MinReviewEase, card.ReviewEase);
    }

    [Fact]
    public void Acertei_atrasado_conta_metade_do_atraso()
    {
        // 10 dias de intervalo, respondida 10 dias depois da data: (10 + 5) × 2,5 = 37,5
        var card = EmRevisao(10);
        card.RecordAnswer(true, T0.AddDays(20));
        Assert.Equal(38, card.ReviewIntervalDays);
        Assert.Equal(T0.AddDays(58), card.NextReviewAt);
    }

    [Fact]
    public void Acertei_antes_da_data_conta_so_o_tempo_que_passou()
    {
        // 2 dias depois, de 10: 2 × 2,5 = 5, menor que o intervalo, que não encolhe
        var cedo = EmRevisao(10);
        cedo.RecordAnswer(true, T0.AddDays(2));
        Assert.Equal(10, cedo.ReviewIntervalDays);
        Assert.Equal(T0.AddDays(12), cedo.NextReviewAt);

        // 8 dias depois, de 10: 8 × 2,5 = 20
        var quase = EmRevisao(10);
        quase.RecordAnswer(true, T0.AddDays(8));
        Assert.Equal(20, quase.ReviewIntervalDays);

        // Mesma ficha duas vezes no mesmo dia: o intervalo não cresce
        var repetida = new Card();
        repetida.RecordAnswer(true, T0);
        repetida.RecordAnswer(true, T0.AddHours(1));
        Assert.Equal(1, repetida.ReviewIntervalDays);
        Assert.Equal(T0.AddHours(25), repetida.NextReviewAt);

        // Estudo diário um pouco antes de completar 24 horas ainda avança: 23 h × 2,5 = 2,4 dias
        var diaria = new Card();
        diaria.RecordAnswer(true, T0);
        diaria.RecordAnswer(true, T0.AddHours(23));
        Assert.Equal(2, diaria.ReviewIntervalDays);
    }

    [Fact]
    public void Resposta_antiga_soma_mas_nao_troca_a_ultima_nem_a_revisao()
    {
        var card = new Card();
        card.RecordAnswer(true, T0);
        card.RecordAnswer(false, T0.AddDays(-5));
        Assert.Equal(1, card.ReviewIntervalDays);
        Assert.Equal(Card.DefaultReviewEase, card.ReviewEase);
        Assert.True(card.LastWasCorrect);
        Assert.Equal(1, card.IncorrectCount);
    }

    [Fact]
    public void O_intervalo_sai_das_datas_inclusive_as_gravadas_pela_caixa_de_Leitner()
    {
        // Como a API anterior deixava a ficha no nível 3: última resposta e, 7 dias depois, a revisão
        var leitner = new Card { LastAnsweredAt = T0, LastWasCorrect = true, NextReviewAt = T0.AddDays(7) };
        Assert.Equal(7, leitner.ReviewIntervalDays);
        // E ela continua de onde estava: 7 dias na data viram 7 × 2,5 = 17,5
        leitner.RecordAnswer(true, T0.AddDays(7));
        Assert.Equal(18, leitner.ReviewIntervalDays);

        Assert.Equal(0, new Card().ReviewIntervalDays);
        Assert.Equal(0, new Card { LastAnsweredAt = T0, LastWasCorrect = false, NextReviewAt = T0 }.ReviewIntervalDays);
    }

    [Fact]
    public void Historico_do_aparelho_e_ajustado()
    {
        var doAparelho = new Card();
        doAparelho.SetHistory(2, 1, T0, true, 2100, T0.AddDays(8));
        Assert.Equal(8, doAparelho.ReviewIntervalDays);
        Assert.Equal(2100, doAparelho.ReviewEase);
        Assert.Equal(T0.AddDays(8), doAparelho.NextReviewAt);

        // Site antigo (caixa de Leitner): manda a data da próxima revisão, sem a facilidade
        var antigo = new Card();
        antigo.SetHistory(2, 1, T0, true, nextReviewAt: T0.AddDays(3));
        Assert.Equal(3, antigo.ReviewIntervalDays);
        Assert.Equal(Card.DefaultReviewEase, antigo.ReviewEase);

        var semData = new Card();
        semData.SetHistory(2, 1, T0, true);
        Assert.Equal(T0.AddDays(1), semData.NextReviewAt);

        // Quem errou por último revisa na hora, diga o aparelho o que disser
        var errou = new Card();
        errou.SetHistory(0, 1, T0, false, reviewEase: 100, nextReviewAt: T0.AddDays(4));
        Assert.Equal(0, errou.ReviewIntervalDays);
        Assert.Equal(Card.MinReviewEase, errou.ReviewEase);
        Assert.Equal(T0, errou.NextReviewAt);

        // Datas fora da faixa: entre 1 dia e 1 ano depois da última resposta
        var longe = new Card();
        longe.SetHistory(1, 0, T0, true, nextReviewAt: T0.AddDays(5000));
        Assert.Equal(T0.AddDays(365), longe.NextReviewAt);
        var antes = new Card();
        antes.SetHistory(1, 0, T0, true, nextReviewAt: T0.AddDays(-2));
        Assert.Equal(T0.AddDays(1), antes.NextReviewAt);

        var incompleto = new Card();
        incompleto.SetHistory(3, 0, null, true, 9000, T0.AddDays(30));
        Assert.Null(incompleto.LastAnsweredAt);
        Assert.Null(incompleto.NextReviewAt);
        Assert.Equal(0, incompleto.ReviewIntervalDays);
        Assert.Equal(Card.DefaultReviewEase, incompleto.ReviewEase);
        Assert.Equal(3, incompleto.CorrectCount);
    }
}
