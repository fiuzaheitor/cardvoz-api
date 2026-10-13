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
        var schemeEnd = value.IndexOf("://", StringComparison.Ordinal);
        if (schemeEnd < 0)
        {
            return value;
        }

        // A URI é lida à mão (e não com System.Uri) para aceitar senhas com
        // caracteres como #, /, ? e @ sem precisar codificá-los: tudo antes do
        // último @ é usuário e senha; o resto é host, porta, banco e opções.
        var rest = value[(schemeEnd + 3)..];
        var at = rest.LastIndexOf('@');
        var userInfo = at >= 0 ? rest[..at] : "";
        var address = at >= 0 ? rest[(at + 1)..] : rest;

        var query = "";
        var queryStart = address.IndexOf('?');
        if (queryStart >= 0)
        {
            query = address[(queryStart + 1)..];
            address = address[..queryStart];
        }

        var database = "";
        var pathStart = address.IndexOf('/');
        if (pathStart >= 0)
        {
            database = Uri.UnescapeDataString(address[(pathStart + 1)..]);
            address = address[..pathStart];
        }

        var (host, port) = SplitHostAndPort(address);

        var separator = userInfo.IndexOf(':');
        var username = separator >= 0 ? userInfo[..separator] : userInfo;
        var password = separator >= 0 ? userInfo[(separator + 1)..] : null;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = Uri.UnescapeDataString(username),
            Password = password is null ? null : Uri.UnescapeDataString(password),
            Database = string.IsNullOrEmpty(database) ? "postgres" : database,
            SslMode = SslMode.Require,
        };

        var sslMode = HttpUtility.ParseQueryString(query)["sslmode"];
        if (!string.IsNullOrEmpty(sslMode)
            && Enum.TryParse<SslMode>(sslMode.Replace("-", ""), ignoreCase: true, out var mode))
        {
            builder.SslMode = mode;
        }

        return builder.ConnectionString;
    }

    private static (string Host, int Port) SplitHostAndPort(string address)
    {
        // IPv6 entre colchetes: [::1]:5432
        var hostEnd = address.StartsWith('[') ? address.IndexOf(']') + 1 : 0;
        var colon = address.IndexOf(':', Math.Max(hostEnd, 0));
        if (colon < 0)
        {
            return (address.Trim('[', ']'), 5432);
        }

        var portText = address[(colon + 1)..];
        if (!int.TryParse(portText, out var port) || port is <= 0 or > 65535)
        {
            throw new FormatException(
                $"Porta inválida na conexão do banco: \"{portText}\". Confira a URI em ConnectionStrings__DefaultConnection.");
        }

        return (address[..colon].Trim('[', ']'), port);
    }
}
