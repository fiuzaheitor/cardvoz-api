using System.Web;
using Npgsql;

namespace cardvoz_api.Data;

/// <summary>
/// Conexão com o banco PostgreSQL (Supabase).
/// </summary>
public static class PostgresSupport
{
    /// <summary>
    /// Aceita a URI que o painel do Supabase fornece
    /// ("postgresql://usuario:senha@host:5432/postgres") ou o formato de chaves
    /// do Npgsql ("Host=...;Username=...;Password=...").
    /// Converte a URI para o formato do Npgsql. Sem "sslmode" na URI, exige SSL,
    /// como o Supabase pede. Strings no formato de chaves são usadas como estão.
    /// </summary>
    public static string NormalizeConnectionString(string connectionString)
    {
        var value = connectionString.Trim();
        if (!value.Contains("://"))
        {
            return value;
        }

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            Database = string.IsNullOrEmpty(database) ? "postgres" : database,
            SslMode = SslMode.Require,
        };

        var sslMode = HttpUtility.ParseQueryString(uri.Query)["sslmode"];
        if (!string.IsNullOrEmpty(sslMode)
            && Enum.TryParse<SslMode>(sslMode.Replace("-", ""), ignoreCase: true, out var mode))
        {
            builder.SslMode = mode;
        }

        return builder.ConnectionString;
    }
}
