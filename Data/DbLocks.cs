using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Data;

/// <summary>
/// Travas do PostgreSQL que valem até o fim da transação (pg_advisory_xact_lock).
/// Servem para dois pedidos ao mesmo tempo não lerem o mesmo valor e um apagar o
/// outro: o EF lê CorrectCount = 5, soma em memória e grava 6; com dois pedidos
/// juntos, os dois gravariam 6 e uma resposta sumiria. Também deixam o "já existe?"
/// do envio repetido (mesmo id) e do slug único valer de verdade.
/// Precisam de uma transação aberta. No SQLite dos testes não fazem nada.
/// </summary>
public static class DbLocks
{
    /// <summary>Acertos e erros das fichas de um baralho (estudo).</summary>
    public static Task LockDeckAsync(this CardVozDbContext db, Guid deckId) => LockAsync(db, $"deck:{deckId}");

    /// <summary>Criar, renomear ou copiar baralhos de uma pessoa (slug único e id repetido).</summary>
    public static Task LockUserDecksAsync(this CardVozDbContext db, Guid userId) => LockAsync(db, $"decks-of:{userId}");

    private static async Task LockAsync(CardVozDbContext db, string key)
    {
        if (!db.Database.IsNpgsql())
            return;
        await db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))");
    }
}
