using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Web.Composizione;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Tests.Unit.Creazione;

/// <summary>D8 e D1b.2-3: con <c>FornitoreAi:DalVivoAbilitato</c> false il prototipo parte e rifiuta solo la generazione con AI.</summary>
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
    public void Con_la_chiave_false_il_prototipo_parte_comunque_senza_chiedere_la_configurazione_dell_AI(bool? dalVivo, string? modalita)
    {
        // D1b.2-3: l'interruttore spento non ferma l'avvio; solo Genera con AI si rifiuta.
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=x",
            ["FornitoreAi:DalVivoAbilitato"] = dalVivo?.ToString(),
            ["FornitoreAi:Creazione:Registrazioni:Modalita"] = modalita,
        }).Build();

        var errori = AvvioPrototipo.Verifica(config, new AmbienteFinto());

        errori.Should().NotContain(e => e.Contains("dal vivo", StringComparison.Ordinal) || e.Contains("Fornitore AI", StringComparison.Ordinal) || e.Contains("Modello", StringComparison.Ordinal));
    }

    [Fact]
    public void Con_la_chiave_true_senza_configurazione_dell_AI_l_avvio_la_chiede()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=x",
            ["FornitoreAi:DalVivoAbilitato"] = "true",
        }).Build();

        AvvioPrototipo.Verifica(config, new AmbienteFinto()).Should().Contain(e => e.Contains("Fornitore AI non configurato", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Con_la_chiave_false_il_fornitore_di_creazione_e_quello_che_rifiuta_senza_chiamate()
    {
        using var host = CliHost.Build(HostRequirements.None, new CliStreams(new StringWriter(), new StringWriter()), b => b.AddInMemoryCollection(
            new Dictionary<string, string?> { ["FornitoreAi:Tipo"] = "claude-code", ["FornitoreAi:Creazione:Modello"] = "claude-opus-5-5" }));
        using var scope = host.Services.CreateScope();

        var fornitore = scope.ServiceProvider.GetRequiredService<IFornitoreAiCreazione>();

        fornitore.Should().BeAssignableTo<IFornitoreAiDisattivato>().Which.Messaggio.Should().Be(FornitoreAiOptions.MessaggioDalVivoDisattivato);
        var chiamata = () => fornitore.CreaAsync(new RichiestaCreazione("anagramma", 1, "{}", "p", "creazione-v1-aaaaaaaaaaaa", "{}"));
        await chiamata.Should().ThrowAsync<InvalidOperationException>();
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
