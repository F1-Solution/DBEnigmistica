using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Console.Cli;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Tests.Unit.FornitoreAi;

/// <summary>Le Registrazioni mal configurate sono un errore di configurazione, mai un ripiego sulle chiamate dal vivo.</summary>
public class RegistrazioniConfigurazioneTests
{
    private static IFornitoreAiEstrazione Risolvi(params (string Chiave, string Valore)[] impostazioni)
    {
        using var host = CliHost.Build(
            HostRequirements.None,
            new CliStreams(new StringWriter(), new StringWriter()),
            builder => builder.AddInMemoryCollection(
                impostazioni
                    .Prepend(("FornitoreAi:Modello", "claude-sonnet-5-5"))
                    .Prepend(("FornitoreAi:Tipo", "claude-code"))
                    .Select(i => new KeyValuePair<string, string?>(i.Item1, i.Item2))));
        using var scope = host.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IFornitoreAiEstrazione>();
    }

    [Theory]
    [InlineData("riproduci")]
    [InlineData("registra")]
    public void ModalitaSenzaCartella_LanciaUnErroreDiConfigurazione(string modalita)
    {
        var risolvi = () => Risolvi(("FornitoreAi:Registrazioni:Modalita", modalita));

        risolvi.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().ContainSingle().Which.Should().Contain("Cartella delle Registrazioni mancante");
    }

    [Fact]
    public void ModalitaSconosciuta_LanciaUnErroreDiConfigurazione()
    {
        var risolvi = () => Risolvi(("FornitoreAi:Registrazioni:Modalita", "boh"), ("FornitoreAi:Registrazioni:Cartella", "x"));

        risolvi.Should().Throw<OptionsValidationException>().Which.Failures.Should().Contain(f => f.Contains("Modalità delle Registrazioni non valida"));
    }

    [Fact]
    public async Task Riproduzione_NonUsaClaudeCode_ESenzaRegistrazioneDaErroreEsplicito()
    {
        var cartella = Path.Combine(Path.GetTempPath(), "dbe-riproduci-" + Guid.NewGuid().ToString("N"));
        var fornitore = Risolvi(
            ("FornitoreAi:Registrazioni:Modalita", "riproduci"),
            ("FornitoreAi:Registrazioni:Cartella", cartella),
            ("FornitoreAi:ClaudeCode:Eseguibile", Path.Combine(cartella, "non-esiste.exe")));

        var richiesta = new RichiestaEstrazionePagina(new string('a', 64), [1], 200, "p", "estrazione-v1-abc", "{}");
        var chiamata = () => fornitore.EstraiPaginaAsync(richiesta);

        fornitore.Should().BeOfType<FornitoreAiRiproduci>();
        await chiamata.Should().ThrowAsync<RegistrazioneMancanteException>();
    }

    private static T RisolviServizio<T>(params (string Chiave, string Valore)[] impostazioni) where T : notnull
    {
        using var host = CliHost.Build(
            HostRequirements.None,
            new CliStreams(new StringWriter(), new StringWriter()),
            builder => builder.AddInMemoryCollection(
                impostazioni
                    .Prepend(("FornitoreAi:Modello", "claude-sonnet-5-5"))
                    .Prepend(("FornitoreAi:Tipo", "claude-code"))
                    .Select(i => new KeyValuePair<string, string?>(i.Item1, i.Item2))));
        using var scope = host.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    [Fact]
    public void D8_ConChiaveDalVivoSpenta_IlFornitoreDiEstrazioneLanciaIlMessaggioDiDisattivazione()
    {
        var risolvi = () => RisolviServizio<IFornitoreAiEstrazione>(("FornitoreAi:DalVivoAbilitato", "false"));

        risolvi.Should().Throw<OptionsValidationException>().Which.Failures.Should().Contain(FornitoreAiOptions.MessaggioDalVivoDisattivato);
    }

    [Fact]
    public void D8_ConChiaveDalVivoSpenta_IlFornitoreDiCreazioneRifiutaSenzaLanciareAllaCostruzione()
    {
        // D1b.2-3: il prototipo parte; solo la generazione con AI si rifiuta.
        var fornitore = RisolviServizio<IFornitoreAiCreazione>(("FornitoreAi:DalVivoAbilitato", "false"));

        fornitore.Should().BeAssignableTo<DBEnigmistica.Application.Creazione.IFornitoreAiDisattivato>()
            .Which.Messaggio.Should().Be(FornitoreAiOptions.MessaggioDalVivoDisattivato);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void D8_ConChiaveDalVivoAccesa_IlFornitoreDalVivoSiCrea(bool creazione)
    {
        var risolvi = () => creazione
            ? (object)RisolviServizio<IFornitoreAiCreazione>(("FornitoreAi:DalVivoAbilitato", "true"))
            : RisolviServizio<IFornitoreAiEstrazione>(("FornitoreAi:DalVivoAbilitato", "true"));

        risolvi.Should().NotThrow();
    }

    [Fact]
    public void D8_LaRiproduzioneDelleRegistrazioniNonRichiedeLaChiaveDalVivo()
    {
        var cartella = Path.Combine(Path.GetTempPath(), "dbe-riproduci-" + Guid.NewGuid().ToString("N"));

        var fornitore = RisolviServizio<IFornitoreAiEstrazione>(
            ("FornitoreAi:DalVivoAbilitato", "false"),
            ("FornitoreAi:Registrazioni:Modalita", "riproduci"),
            ("FornitoreAi:Registrazioni:Cartella", cartella));

        fornitore.Should().BeOfType<FornitoreAiRiproduci>();
    }
}
