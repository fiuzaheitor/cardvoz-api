namespace cardvoz_api.Auth;

public static class RateLimitPolicies
{
    /// <summary>Limite por IP para cadastro, login e renovação (contra tentativas em massa).</summary>
    public const string Auth = "auth";
}
