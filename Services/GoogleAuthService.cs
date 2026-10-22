using System.Text.Json.Serialization;
using cardvoz_api.Auth;
using Google.Apis.Auth;

namespace cardvoz_api.Services;

/// <summary>
/// Fluxo "authorization code + PKCE" do Google. O site só recebe um código de
/// uso único; a troca pelo id_token acontece aqui, com o segredo do cliente.
/// </summary>
public class GoogleAuthService : IGoogleAuthService
{
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    private readonly HttpClient _httpClient;
    private readonly GoogleSettings _settings;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(HttpClient httpClient, GoogleSettings settings, ILogger<GoogleAuthService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task<GoogleIdentity?> ExchangeCodeAsync(
        string code, string codeVerifier, string redirectUri, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["code_verifier"] = codeVerifier,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = _settings.ClientId!,
            ["client_secret"] = _settings.ClientSecret!,
        }), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Código vencido/já usado, verificador errado ou endereço não cadastrado.
            // O corpo traz só o tipo do erro (sem dados da pessoa), útil para diagnosticar.
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Google recusou a troca do código ({Status}): {Body}", (int)response.StatusCode, body);
            return null;
        }

        var tokens = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken);
        if (string.IsNullOrEmpty(tokens?.IdToken))
        {
            _logger.LogWarning("Resposta do Google sem id_token.");
            return null;
        }

        try
        {
            // Confere assinatura, emissor (Google), validade e se o token foi emitido para este cliente
            var payload = await GoogleJsonWebSignature.ValidateAsync(tokens.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _settings.ClientId! },
            });

            if (string.IsNullOrEmpty(payload.Subject) || string.IsNullOrEmpty(payload.Email))
            {
                return null;
            }

            return new GoogleIdentity(payload.Subject, payload.Email, payload.EmailVerified, payload.Name);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning("id_token do Google inválido: {Motivo}", ex.Message);
            return null;
        }
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }
}
