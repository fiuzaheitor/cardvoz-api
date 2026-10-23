namespace cardvoz_api.Auth;

public static class RateLimitPolicies
{
    /// <summary>
    /// Limite por IP para cadastro e login com senha (contra adivinhação de senha):
    /// 10 por minuto.
    /// </summary>
    public const string Auth = "auth";

    /// <summary>
    /// Limite por IP para login com Google, renovação e logout: 60 por minuto. Não há
    /// senha para adivinhar, e numa rede compartilhada (Wi-Fi da escola) todo mundo
    /// sai pelo mesmo IP: cada página recarregada é uma renovação.
    /// </summary>
    public const string Session = "session";
}
