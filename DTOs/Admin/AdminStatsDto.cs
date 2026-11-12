namespace cardvoz_api.DTOs.Admin;

public class AdminStatsDto
{
    public int Users { get; set; }
    public int Admins { get; set; }
    public int Decks { get; set; }
    public int SharedDecks { get; set; }
    public int Cards { get; set; }
    public int StudySessions { get; set; }

    /// <summary>Respostas "Acertei" e "Errei" somadas de todas as fichas das pessoas.</summary>
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }

    /// <summary>Fichas que chegaram na data de revisão (aparecem em "Revisar agora").</summary>
    public int CardsDueNow { get; set; }

    /// <summary>Pessoas que estudaram nos últimos 7 dias.</summary>
    public int ActiveLast7Days { get; set; }

    /// <summary>Contas com o e-mail confirmado.</summary>
    public int ConfirmedEmails { get; set; }
}
