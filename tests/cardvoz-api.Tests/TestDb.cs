using cardvoz_api.Data;
using cardvoz_api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Tests;

/// <summary>
/// Banco SQLite em memória, um por teste. Relacional como o PostgreSQL (aceita
/// ExecuteUpdate/ExecuteDelete, chaves estrangeiras e cascata), sem precisar de servidor.
/// </summary>
public static class TestDb
{
    public static CardVozDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<CardVozDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new CardVozDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static User AddUser(CardVozDbContext db, string email = "ana@exemplo.com", bool isAdmin = false)
    {
        var user = new User { Name = "Ana", Email = email, IsAdmin = isAdmin };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public static Deck AddDeck(CardVozDbContext db, User user, params string[] questions)
    {
        var deck = new Deck { UserId = user.Id, Title = "Biologia", Slug = "biologia-" + Guid.NewGuid().ToString("N")[..6], Category = "Geral" };
        deck.Cards = questions.Select((q, i) => new Card { DeckId = deck.Id, Question = q, Answer = "r", DisplayOrder = i + 1 }).ToList();
        db.Decks.Add(deck);
        db.SaveChanges();
        return deck;
    }
}
