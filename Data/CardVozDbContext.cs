using Microsoft.EntityFrameworkCore;
using cardvoz_api.Models;

namespace cardvoz_api.Data;

public class CardVozDbContext : DbContext
{
    public CardVozDbContext(DbContextOptions<CardVozDbContext> options) : base(options)
    {
    }

    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<StudySession> StudySessions => Set<StudySession>();
    public DbSet<StudyReview> StudyReviews => Set<StudyReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Deck Configuration
        modelBuilder.Entity<Deck>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.Slug).IsUnique();
            entity.Property(d => d.Slug).IsRequired().HasMaxLength(150);
            entity.Property(d => d.Title).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Category).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Description).HasMaxLength(1000);

            entity.HasMany(d => d.Cards)
                .WithOne(c => c.Deck)
                .HasForeignKey(c => c.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.StudySessions)
                .WithOne(s => s.Deck)
                .HasForeignKey(s => s.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Card Configuration
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Question).IsRequired();
            entity.Property(c => c.Answer).IsRequired();
            entity.Property(c => c.VoiceNotes).HasMaxLength(500);

            entity.HasOne(c => c.Deck)
                .WithMany(d => d.Cards)
                .HasForeignKey(c => c.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StudySession Configuration
        modelBuilder.Entity<StudySession>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.HasOne(s => s.Deck)
                .WithMany(d => d.StudySessions)
                .HasForeignKey(s => s.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.Reviews)
                .WithOne(r => r.StudySession)
                .HasForeignKey(r => r.StudySessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StudyReview Configuration
        modelBuilder.Entity<StudyReview>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.StudySession)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.StudySessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Card)
                .WithMany()
                .HasForeignKey(r => r.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
