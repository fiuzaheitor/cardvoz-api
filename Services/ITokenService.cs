using cardvoz_api.Models;

namespace cardvoz_api.Services;

public interface ITokenService
{
    /// <summary>Gera o token de acesso (JWT) e sua data de expiração.</summary>
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user);

    /// <summary>Gera um token de renovação aleatório e o hash que vai para o banco.</summary>
    (string Token, string Hash) CreateRefreshToken();

    /// <summary>Hash SHA-256 (hex) de um token de renovação recebido.</summary>
    string HashRefreshToken(string token);
}
