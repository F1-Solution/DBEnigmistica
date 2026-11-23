using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Web.Composizione;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Tests.Unit.Creazione;

/// <summary>D8: <c>dbe prototipo</c> non parte per la generazione dal vivo se <c>FornitoreAi:DalVivoAbilitato</c> è false.</summary>
public sealed class InterruttoreDalVivoPrototipoTests
{
    private static IConfiguration Config(bool? dalVivo, string? modalita) => new ConfigurationBuilder().AddInMemoryCollection(
        new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=x",
            ["FornitoreAi:Tipo"] = "claude-code",
            ["FornitoreAi:Creazione:Modello"] = "claude-opus-5-5",
            ["FornitoreAi:DalVivoAbilitato"] = dalVivo?.ToString(),
            ["FornitoreAi:Creazione:Registrazioni:Modalita"] = modalita,
            ["FornitoreAi:Creazione:Registrazioni:Cartella"] = modalita is null ? null : "reg",
        }).Build();

    [Theory]
    [InlineData(null, null)]
    [InlineData(false, null)]
    [InlineData(false, "registra")]
    public void Con_la_chiave_false_la_generazione_dal_vivo_e_rifiutata(bool? dalVivo, string? modalita)
    {
        AvvioPrototipo.Verifica(Config(dalVivo, modalita), new AmbienteFinto()).Should().Contain(FornitoreAiOptions.MessaggioDalVivoDisattivato);
    }

    [Fact]
    public async Task EseguiAsync_con_la_chiave_false_lancia_prima_di_avviare_l_host()
    {
        var azione = () => AvvioPrototipo.EseguiAsync(Config(false, "registra"), new StringWriter(), CancellationToken.None);

        (await azione.Should().ThrowAsync<OptionsValidationException>()).Which.Failures.Should().Contain(FornitoreAiOptions.MessaggioDalVivoDisattivato);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(true, "registra")]
    [InlineData(false, "riproduci")]
    public void Con_la_chiave_true_o_in_riproduzione_il_prototipo_prosegue(bool dalVivo, string? modalita)
    {
        AvvioPrototipo.Verifica(Config(dalVivo, modalita), new AmbienteFinto()).Should().NotContain(FornitoreAiOptions.MessaggioDalVivoDisattivato);
    }

    private sealed class AmbienteFinto : IAmbienteFornitoreAi
    {
        public string CartellaUtente => "utente";

        public bool EWindows => false;

        public string? Variabile(string nome) => nome == "CLAUDE_CODE_OAUTH_TOKEN" ? "t" : null;

        public bool FileEsiste(string percorso) => false;

        public bool EseguibileNelPath(string nome) => true;
    }
}
