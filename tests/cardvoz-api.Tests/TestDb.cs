using cardvoz_api.Data;
using cardvoz_api.Models;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Tests;

/// <summary>Banco em memória, um por teste, com uma pessoa e um baralho prontos.</summary>
public static class TestDb
{
    public static CardVozDbContext Create()
    {
        var options = new DbContextOptionsBuilder<CardVozDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CardVozDbContext(options);
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
