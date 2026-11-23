using DBEnigmistica.AI.Configuration;
using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Valutazione;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Commands;
using DBEnigmistica.Infrastructure.Services.Registrazioni;
using DBEnigmistica.Tests.Fixtures.ClaudeFinto;
using DBEnigmistica.Tests.Unit.Creazione;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static DBEnigmistica.Tests.Unit.Valutazione.ServiziDiProva;

namespace DBEnigmistica.Tests.Unit.Valutazione;

[Collection("AmbienteProcesso")]
public sealed class EvalHandlerTests : IDisposable
{
    private readonly string _radice = Path.Combine(Path.GetTempPath(), "dbe-handler-" + Guid.NewGuid().ToString("N"));
    private readonly StringWriter _out = new();
    private readonly StringWriter _err = new();

    public void Dispose()
    {
        if (Directory.Exists(_radice))
            Directory.Delete(_radice, recursive: true);
    }

    private EvalExtractionHandler Handler(IServiceProvider? provider = null, bool dalVivoAbilitato = true) =>
        new(Servizio(), new FabbricaArchivioRegistrazioniFileSystem(), new InterpreteRispostaClaudeCode(), new ValidatoreSchemaJson(),
            provider ?? new ServiceCollection().BuildServiceProvider(),
            Options.Create(new FornitoreAiOptions { DalVivoAbilitato = dalVivoAbilitato }), new CliStreams(_out, _err));

    private string CopiaSet()
    {
        var destinazione = Path.Combine(_radice, "set");
        CopiaCartella(SetVersionato, destinazione);
        return destinazione;
    }

    [Fact]
    public async Task Replay_SulSetSintetico_EsceZeroEScriveIlRapporto()
    {
        var uscita = new DirectoryInfo(Path.Combine(_radice, "uscita"));

        var codice = await Handler().ExecuteAsync(Parametri(SetVersionato), replay: true, live: false, limite: null, uscita, CancellationToken.None);

        codice.Should().Be(ExitCodes.Success);
        _out.ToString().Should().Contain("Rapporto della regressione");
        File.Exists(Path.Combine(uscita.FullName, "rapporto.json")).Should().BeTrue();
        File.Exists(Path.Combine(uscita.FullName, "rapporto.md")).Should().BeTrue();
    }

    [Fact]
    public async Task Replay_ConUnaMetricaCheScende_EsceDiversoDaZeroEDiceQualeMetrica()
    {
        var cartella = CopiaSet();
        var rapporto = (await Servizio().EseguiAsync(Parametri(cartella), Riproduci(cartella))).Rapporto;
        await File.WriteAllTextAsync(Path.Combine(cartella, "linea-di-base.json"), LineaDiBase.DaRapporto(rapporto).Con("totale", "precisione", 1.0).InJson());

        var codice = await Handler().ExecuteAsync(Parametri(cartella), replay: true, live: false, limite: null, null, CancellationToken.None);

        codice.Should().Be(ExitCodes.Error);
        _out.ToString().Should().Contain("precisione");
    }

    [Fact]
    public async Task Replay_SenzaLineaDiBase_EsceZeroEStampaCheIlConfrontoManca()
    {
        var cartella = CopiaSet();
        File.Delete(Path.Combine(cartella, "linea-di-base.json"));

        var codice = await Handler().ExecuteAsync(Parametri(cartella), replay: true, live: false, limite: null, null, CancellationToken.None);

        codice.Should().Be(ExitCodes.Success);
        _out.ToString().Should().Contain("Linea di base assente");
    }

    [Fact]
    public async Task Replay_ConUnaRegistrazioneMancante_EsceDiversoDaZeroConUnMessaggioInItaliano()
    {
        var cartella = CopiaSet();
        Directory.Delete(Path.Combine(cartella, "registrazioni"), recursive: true);

        var codice = await Handler().ExecuteAsync(Parametri(cartella), replay: true, live: false, limite: null, null, CancellationToken.None);

        codice.Should().Be(ExitCodes.Error);
        _err.ToString().Should().Contain("Registrazione mancante");
    }

    [Theory]
    [InlineData(false, false, null)]
    [InlineData(true, true, 5)]
    [InlineData(false, true, null)]
    [InlineData(false, true, 0)]
    public async Task ModalitaNonValida_EsceDiversoDaZero(bool replay, bool live, int? limite)
    {
        var codice = await Handler().ExecuteAsync(Parametri(SetVersionato), replay, live, limite, null, CancellationToken.None);

        codice.Should().Be(ExitCodes.Error);
        _err.ToString().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Live_ConLimite_UsaIlFornitoreRegistratoESiFermaAlLimite()
    {
        var cartella = CopiaSet();
        File.Delete(Path.Combine(cartella, "linea-di-base.json"));
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(
            "{\"type\":\"result\",\"is_error\":false,\"result\":\"\",\"structured_output\":" + File.ReadAllText(Path.Combine(SetVersionato, "risposte", "s01.json")) + "}"));
        var servizi = new ServiceCollection();
        servizi.AddSingleton<IFornitoreAiEstrazione>(new FornitoreClaudeCode(
            new ImpostazioniClaudeCode(finto.Eseguibile, "claude-finto", TimeSpan.FromSeconds(120), 1, TimeSpan.FromSeconds(1)), new ValidatoreSchemaJson()));

        var azione = () => Handler(servizi.BuildServiceProvider()).ExecuteAsync(Parametri(cartella), replay: false, live: true, limite: 1, null, CancellationToken.None);

        await azione.Should().ThrowAsync<LimiteEsecuzioneRaggiuntoException>();
        finto.Invocazioni.Should().HaveCount(1);
    }

    [Fact]
    public async Task Live_ConChiaveDalVivoAbilitatoFalse_RifiutaSenzaChiamareIlFornitore()
    {
        var servizi = new ServiceCollection();
        servizi.AddSingleton<IFornitoreAiEstrazione>(_ => throw new InvalidOperationException("Il fornitore non deve essere risolto."));

        var codice = await Handler(servizi.BuildServiceProvider(), dalVivoAbilitato: false)
            .ExecuteAsync(Parametri(SetVersionato), replay: false, live: true, limite: 1, null, CancellationToken.None);

        codice.Should().Be(ExitCodes.Configuration);
        _err.ToString().Trim().Should().Be(FornitoreAiOptions.MessaggioDalVivoDisattivato);
        _out.ToString().Should().BeEmpty();
    }

    [Fact]
    public void ILavoriDalVivoSonoDisattivatiDiDefaultEdEsplicitiInAppsettings()
    {
        new FornitoreAiOptions().DalVivoAbilitato.Should().BeFalse();
        FornitoreAiOptions.MessaggioDalVivoDisattivato.Should().Be(
            "Le chiamate dal vivo sono disattivate. Si abilitano con FornitoreAi:DalVivoAbilitato=true solo dopo la prova 1.1-prova (PROGRESS.md).");
        var json = File.ReadAllText(FileDelRepository.Percorso("src", "DBEnigmistica.Console", "appsettings.json"));
        System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("FornitoreAi"u8).GetProperty("DalVivoAbilitato"u8).GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Replay_ConChiaveDalVivoAbilitatoFalse_NonCambia()
    {
        var codice = await Handler(dalVivoAbilitato: false).ExecuteAsync(Parametri(SetVersionato), replay: true, live: false, limite: null, null, CancellationToken.None);

        codice.Should().Be(ExitCodes.Success);
    }

    [Fact]
    public async Task Bozza_ScriveLeBozzeEStampaQuelleSaltate()
    {
        var cartella = CopiaSet();
        File.Delete(Path.Combine(cartella, "attesi", "s03.json"));
        var handler = new EvalDraftHandler(Bozze(), new CliStreams(_out, _err));

        var codice = await handler.ExecuteAsync(cartella, "finto", "finto-1", 200, sovrascrivi: false, CancellationToken.None);

        codice.Should().Be(ExitCodes.Success);
        _out.ToString().Should().Contain("s03").And.Contain("Bozze scritte: 1");
        File.Exists(Path.Combine(cartella, "attesi", "s03.json")).Should().BeTrue();
    }
}
