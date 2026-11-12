using cardvoz_api.Auth;
using Xunit;

namespace cardvoz_api.Tests;

public class SettingsTests
{
    [Theory]
    [InlineData("https://cardvoz.onrender.com/entrar/", true)]
    [InlineData("https://cardvoz-admin.onrender.com/entrar/", true)]
    [InlineData("https://cardvoz.onrender.com/entrar/?x=1", false)]
    [InlineData("https://cardvoz.onrender.com/outra/", false)]
    [InlineData("https://site-falso.com/entrar/", false)]
    public void Google_so_volta_para_o_entrar_de_origens_liberadas(string uri, bool allowed)
    {
        var settings = new GoogleSettings { AllowedOrigins = new[] { "https://cardvoz.onrender.com", "https://cardvoz-admin.onrender.com" } };
        Assert.Equal(allowed, settings.IsAllowedRedirectUri(uri));
    }

    [Fact]
    public void Email_so_fica_configurado_com_servidor_remetente_e_site()
    {
        Assert.False(new EmailSettings { Host = "smtp.gmail.com", From = "x@y.com" }.IsConfigured);
        Assert.True(new EmailSettings { Host = "smtp.gmail.com", From = "x@y.com", SiteUrl = "https://cardvoz.onrender.com" }.IsConfigured);
    }
}
