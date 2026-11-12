using cardvoz_api.Data;
using cardvoz_api.DTOs.Account;
using cardvoz_api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cardvoz_api.Services;

public enum DeleteAccountResult
{
    Deleted,
    NotFound,
    WrongEmail,
    WrongPassword,
    /// <summary>Única conta de admin: o painel ficaria sem ninguém para gerenciá-lo.</summary>
    LastAdmin
}

/// <summary>
/// Direitos da pessoa sobre os próprios dados (LGPD): baixar tudo o que a conta guarda
/// e excluir a conta.
/// </summary>
public class AccountService
{
    private readonly CardVozDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AccountService> _logger;

    public AccountService(CardVozDbContext context, IPasswordHasher<User> passwordHasher, ILogger<AccountService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Tudo o que a conta guarda, em JSON legível. Sem hash de senha nem tokens: não
    /// são dados da pessoa, são a segurança da conta.
    /// </summary>
    public async Task<object?> ExportAsync(Guid userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return null;

        var decks = await _context.Decks
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .Include(d => d.Cards.OrderBy(c => c.DisplayOrder))
            .Include(d => d.StudySessions).ThenInclude(s => s.Reviews)
            .AsSplitQuery()
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();

        return new
        {
            exportedAt = DateTime.UtcNow,
            account = new
            {
                name = user.Name,
                email = user.Email,
                createdAt = user.CreatedAt,
                signsInWithPassword = user.PasswordHash != null,
                signsInWithGoogle = user.GoogleSubject != null,
                isAdmin = user.IsAdmin
            },
            decks = decks.Select(d =>
            {
                var questions = d.Cards.ToDictionary(c => c.Id, c => c.Question);
                return new
                {
                    title = d.Title,
                    category = d.Category,
                    description = d.Description,
                    questionLanguage = d.QuestionLanguage,
                    answerLanguage = d.AnswerLanguage,
                    createdAt = d.CreatedAt,
                    updatedAt = d.UpdatedAt,
                    cards = d.Cards.Select(c => new
                    {
                        question = c.Question,
                        answer = c.Answer,
                        correctCount = c.CorrectCount,
                        incorrectCount = c.IncorrectCount,
                        lastAnsweredAt = c.LastAnsweredAt,
                        lastWasCorrect = c.LastWasCorrect,
                        reviewLevel = c.ReviewLevel,
                        nextReviewAt = c.NextReviewAt
                    }),
                    studySessions = d.StudySessions.OrderBy(s => s.StartedAt).Select(s => new
                    {
                        startedAt = s.StartedAt,
                        completedAt = s.CompletedAt,
                        totalCards = s.TotalCards,
                        correctCount = s.CorrectCount,
                        incorrectCount = s.IncorrectCount,
                        answers = s.Reviews.OrderBy(r => r.AnsweredAt).Select(r => new
                        {
                            question = questions.GetValueOrDefault(r.CardId),
                            wasCorrect = r.WasCorrect,
                            answeredAt = r.AnsweredAt
                        })
                    })
                };
            })
        };
    }

    /// <summary>Exclui a conta com baralhos, fichas, sessões e tokens (o banco apaga em cascata).</summary>
    public async Task<DeleteAccountResult> DeleteAsync(Guid userId, DeleteAccountDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return DeleteAccountResult.NotFound;

        if (!string.Equals(dto.ConfirmEmail.Trim(), user.Email, StringComparison.OrdinalIgnoreCase))
            return DeleteAccountResult.WrongEmail;

        if (user.PasswordHash != null)
        {
            var check = string.IsNullOrEmpty(dto.Password)
                ? PasswordVerificationResult.Failed
                : _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (check == PasswordVerificationResult.Failed)
                return DeleteAccountResult.WrongPassword;
        }

        if (user.IsAdmin && await _context.Users.CountAsync(u => u.IsAdmin) == 1)
            return DeleteAccountResult.LastAdmin;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Conta excluída pela própria pessoa: {UserId}", userId);
        return DeleteAccountResult.Deleted;
    }
}
