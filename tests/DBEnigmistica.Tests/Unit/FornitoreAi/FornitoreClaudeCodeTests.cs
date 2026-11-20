using System.Diagnostics;
using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Tests.Fixtures.ClaudeFinto;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.FornitoreAi;

/// <summary>Adattatore Claude Code provato con un eseguibile finto: nessuna chiamata di rete.</summary>
[Collection("AmbienteProcesso")]
public class FornitoreClaudeCodeTests
{
    private const string Schema = "{\"type\":\"object\",\"properties\":{\"nome\":{\"type\":\"string\"}},\"required\":[\"nome\"]}";

    private const string RispostaValida =
        "{\"type\":\"result\",\"subtype\":\"success\",\"is_error\":false,\"result\":\"\",\"structured_output\":{\"nome\":\"x\"},"
        + "\"total_cost_usd\":0.0123,\"usage\":{\"input_tokens\":100,\"output_tokens\":20}}";

    private const string Impronta = "abababababababababababababababababababababababababababababababab";

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 1, 2, 3, 4, 5];

    private const string Prompt = "Leggi la pagina\ncon testo nativo e catalogo\n<<<TESTO\nnativo\nTESTO>>>";

    private const string RigaImmagine =
        "L'immagine della pagina è nel file pagina.png nella cartella corrente: leggila con lo strumento Read.";

    private static RichiestaEstrazionePagina Richiesta() =>
        new(Impronta, Png, 200, Prompt, "v1", Schema);

    private static FornitoreClaudeCode Crea(
        ClaudeFintoIstanza finto,
        IValidatoreSchemaJson? validatore = null,
        TimeSpan? timeout = null,
        int tentativi = 3,
        List<TimeSpan>? attese = null,
        TimeProvider? tempo = null) =>
        new(
            new ImpostazioniClaudeCode(finto.Eseguibile, "claude-sonnet-5-5", timeout ?? TimeSpan.FromSeconds(60), tentativi, TimeSpan.FromSeconds(2)),
            validatore ?? new ValidatoreSempre([]),
            tempo,
            (durata, _) =>
            {
                attese?.Add(durata);
                return Task.CompletedTask;
            });

    [Fact]
    public async Task RispostaValida_RestituisceRiuscitaConMetadati()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));

        var risposta = await Crea(finto).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.Riuscita);
        risposta.Json.Should().Be("{\"nome\":\"x\"}");
        var chiamata = risposta.Chiamate.Single();
        chiamata.Fornitore.Should().Be("claude-code");
        chiamata.Modello.Should().Be("claude-sonnet-5-5");
        chiamata.TokenIngresso.Should().Be(100);
        chiamata.TokenUscita.Should().Be(20);
        chiamata.Costo.Should().Be(0.0123m);
        chiamata.CostoValuta.Should().Be("USD");
        chiamata.CostoTipo.Should().Be("indicativo");
        chiamata.ImprontaPagina.Should().Equal(Convert.FromHexString(Impronta));
        chiamata.Uso.Should().Be("estrazione");
        chiamata.Tentativo.Should().Be(1);
        chiamata.Correzione.Should().BeFalse();
        chiamata.TestoNativoTroncato.Should().BeFalse();
        chiamata.VersionePrompt.Should().Be("v1");
        chiamata.RispostaGrezza.Should().Be(RispostaValida);
        chiamata.RispostaSha256.Should().HaveCount(32);
        finto.Invocazioni.Should().HaveCount(1);
    }

    [Fact]
    public async Task RispostaNonConforme_ChiedeUnaCorrezioneEPoiDiceRispostaNonValida()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));
        var validatore = new ValidatoreSempre(["manca il campo nome"]);

        var risposta = await Crea(finto, validatore).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.RispostaNonValida);
        risposta.Messaggio.Should().Contain("manca il campo nome");
        var invocazioni = finto.Invocazioni;
        invocazioni.Should().HaveCount(2);
        invocazioni[1].Stdin.Should().Contain("manca il campo nome");
    }

    [Fact]
    public async Task RispostaNonConforme_RegistraDueChiamateLaSecondaECorrezione()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));

        var risposta = await Crea(finto, new ValidatoreSempre(["errore"])).EstraiPaginaAsync(Richiesta());

        risposta.Chiamate.Select(c => (c.Tentativo, c.Correzione, c.Esito)).Should().Equal(
            (1, false, EsitoEstrazione.RispostaNonValida),
            (2, true, EsitoEstrazione.RispostaNonValida));
        risposta.Chiamate.Select(c => c.EsitoTesto).Should().OnlyContain(e => e == "risposta_non_valida");
    }

    [Fact]
    public async Task ErroreTransitorio_RegistraUnaChiamataPerOgniTentativo()
    {
        var sovraccarico = "{\"type\":\"result\",\"is_error\":true,\"result\":\"API Error: 529 Overloaded\"}";
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(sovraccarico, Exit: 1), new PassoClaudeFinto(RispostaValida));

        var risposta = await Crea(finto).EstraiPaginaAsync(Richiesta());

        risposta.Chiamate.Select(c => c.Esito).Should().Equal(EsitoEstrazione.ErroreTransitorio, EsitoEstrazione.Riuscita);
        risposta.Chiamate[0].CodiceUscita.Should().Be(1);
        risposta.Chiamate[0].MessaggioErrore.Should().NotBeNullOrEmpty();
        risposta.Chiamate[1].MessaggioErrore.Should().BeNull();
    }

    [Fact]
    public async Task TestoNativoTroncato_VieneMarcatoNellaChiamata()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));

        var risposta = await Crea(finto).EstraiPaginaAsync(Richiesta() with { TestoNativoTroncato = true });

        risposta.Chiamate.Single().TestoNativoTroncato.Should().BeTrue();
    }

    [Fact]
    public async Task ConValidatoreVero_RispostaFuoriSchema_UnaCorrezioneEPoiRispostaNonValida()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));
        const string schemaRigido = "{\"type\":\"object\",\"required\":[\"altro\"]}";

        var risposta = await Crea(finto, new ValidatoreSchemaJson())
            .EstraiPaginaAsync(Richiesta() with { JsonSchema = schemaRigido });

        risposta.Esito.Should().Be(EsitoEstrazione.RispostaNonValida);
        finto.Invocazioni.Should().HaveCount(2);
        risposta.Chiamate.Should().HaveCount(2);
    }

    [Fact]
    public async Task RispostaNonConformeCorrettaAlSecondoTentativo_Riesce()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));
        var validatore = new ValidatoreSequenza([["errore"], []]);

        var risposta = await Crea(finto, validatore).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.Riuscita);
        finto.Invocazioni.Should().HaveCount(2);
    }

    [Fact]
    public async Task FinestraEsaurita_DaEsitoDedicatoConLOraDiRipresaESenzaRetry()
    {
        const string testo = "You've hit your session limit · resets 3:45pm";
        var uscita = "{\"type\":\"result\",\"subtype\":\"success\",\"is_error\":true,\"result\":\"" + testo + "\"}";
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(uscita, Exit: 1));
        var tempo = new TempoFisso(new DateTimeOffset(2026, 11, 20, 10, 0, 0, TimeSpan.Zero));

        var risposta = await Crea(finto, tempo: tempo).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.FinestraEsaurita);
        risposta.RipresaFinestra.Should().Be(new DateTimeOffset(2026, 11, 20, 15, 45, 0, TimeSpan.Zero));
        finto.Invocazioni.Should().HaveCount(1);
    }

    [Fact]
    public async Task FinestraEsaurita_ConOraGiaPassata_RipresaIlGiornoDopo()
    {
        var uscita = "{\"type\":\"result\",\"is_error\":true,\"result\":\"You've hit your session limit · resets 3:45pm\"}";
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(uscita, Exit: 1));
        var tempo = new TempoFisso(new DateTimeOffset(2026, 11, 20, 18, 0, 0, TimeSpan.Zero));

        var risposta = await Crea(finto, tempo: tempo).EstraiPaginaAsync(Richiesta());

        risposta.RipresaFinestra.Should().Be(new DateTimeOffset(2026, 11, 21, 15, 45, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task FinestraEsaurita_SenzaOraRiconoscibile_NonHaLaRipresa()
    {
        var uscita = "{\"type\":\"result\",\"is_error\":true,\"result\":\"You've hit your weekly limit\"}";
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(uscita, Exit: 1));

        var risposta = await Crea(finto).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.FinestraEsaurita);
        risposta.RipresaFinestra.Should().BeNull();
    }

    [Fact]
    public async Task ErroreTransitorio_RitentaConAttesaCrescenteEPoiRiesce()
    {
        var sovraccarico = "{\"type\":\"result\",\"is_error\":true,\"result\":\"API Error: Repeated 529 Overloaded errors. The API is at capacity\"}";
        using var finto = ClaudeFintoFixture.Crea(
            new PassoClaudeFinto(sovraccarico, Exit: 1),
            new PassoClaudeFinto(sovraccarico, Exit: 1),
            new PassoClaudeFinto(RispostaValida));
        var attese = new List<TimeSpan>();

        var risposta = await Crea(finto, attese: attese).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.Riuscita);
        finto.Invocazioni.Should().HaveCount(3);
        attese.Should().Equal(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4));
    }

    [Fact]
    public async Task ErroreTransitorioSempre_DopoITentativiRestaTransitorio()
    {
        var sovraccarico = "{\"type\":\"result\",\"is_error\":true,\"result\":\"Server is temporarily limiting requests\"}";
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(sovraccarico, Exit: 1));

        var risposta = await Crea(finto, tentativi: 2).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.ErroreTransitorio);
        finto.Invocazioni.Should().HaveCount(2);
    }

    [Fact]
    public async Task Timeout_DaErroreTransitorioRitentaEChiudeIlProcesso()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto("", SleepMs: 60_000));

        var risposta = await Crea(finto, timeout: TimeSpan.FromSeconds(3), tentativi: 2).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.ErroreTransitorio);
        var invocazioni = finto.Invocazioni;
        invocazioni.Should().HaveCount(2);
        foreach (var invocazione in invocazioni)
            ProcessoTerminato(invocazione.Pid).Should().BeTrue("il processo scaduto va chiuso");
    }

    [Fact]
    public async Task Timeout_ConUnNipoteCheTieneApertaLaPipe_ScattaEntroIlTimeout()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida, Nipote: true));
        var cronometro = Stopwatch.StartNew();

        var risposta = await Crea(finto, timeout: TimeSpan.FromSeconds(2), tentativi: 1).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.ErroreTransitorio);
        risposta.Messaggio.Should().Contain("Timeout");
        cronometro.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(7), "il nipote vive circa 9 secondi");
    }

    [Fact]
    public async Task ErroreDiAutenticazione_EDefinitivoESenzaRetry()
    {
        var uscita = "{\"type\":\"result\",\"is_error\":true,\"result\":\"Not logged in · Please run /login\"}";
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(uscita, Exit: 1));

        var risposta = await Crea(finto).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.ErroreDefinitivo);
        finto.Invocazioni.Should().HaveCount(1);
    }

    [Fact]
    public async Task UscitaNonJson_EErroreDefinitivo()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto("non e' json", Exit: 1));

        var risposta = await Crea(finto).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.ErroreDefinitivo);
    }

    [Fact]
    public async Task Argomenti_SonoUnElencoENonUnaStringaDiShell()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));

        await Crea(finto).EstraiPaginaAsync(Richiesta());

        var argomenti = finto.Invocazioni.Single().Args;
        argomenti.Should().Contain("-p");
        argomenti.Should().ContainInOrder("--output-format", "json");
        argomenti.Should().ContainInOrder("--model", "claude-sonnet-5-5");
        var posizione = Array.IndexOf(argomenti, "--json-schema");
        posizione.Should().BeGreaterThan(-1);
        argomenti[posizione + 1].Should().Be(Schema, "virgolette e spazi devono arrivare intatti");
    }

    [Fact]
    public async Task Prompt_ArrivaDaStdinEMaiNegliArgomenti()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));

        await Crea(finto).EstraiPaginaAsync(Richiesta());

        var invocazione = finto.Invocazioni.Single();
        invocazione.Stdin.Should().Contain("Leggi la pagina").And.Contain("nativo").And.Contain("catalogo");
        string.Join(' ', invocazione.Args).Should().NotContain("Leggi la pagina");
    }

    [Fact]
    public async Task Prompt_ELIstruzioneDelModelloPiuSoloLaRigaDellImmagine()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));

        await Crea(finto).EstraiPaginaAsync(Richiesta());

        finto.Invocazioni.Single().Stdin.Should().Be(Prompt + "\n\n" + RigaImmagine);
    }

    [Fact]
    public async Task Argomenti_LimitanoReadAlSoloFileDellImmagineENonCaricanoLeImpostazioniUtente()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));

        await Crea(finto).EstraiPaginaAsync(Richiesta());

        var argomenti = finto.Invocazioni.Single().Args;
        argomenti.Should().ContainInOrder("--allowedTools", "Read(./pagina.png)");
        argomenti.Should().ContainInOrder("--tools", "Read");
        argomenti.Should().ContainInOrder("--setting-sources", "project");
    }

    [Fact]
    public async Task JsonSchema_ViaggiaMinimizzato()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));
        const string rientrato = "{\n  \"type\": \"object\",\n  \"required\": [ \"nome\" ]\n}";

        await Crea(finto).EstraiPaginaAsync(Richiesta() with { JsonSchema = rientrato });

        var argomenti = finto.Invocazioni.Single().Args;
        argomenti[Array.IndexOf(argomenti, "--json-schema") + 1].Should().Be("{\"type\":\"object\",\"required\":[\"nome\"]}");
    }

    [Fact]
    public async Task JsonSchemaTroppoLungoPerLaRigaDiComando_DaErroreDefinitivoSenzaAvviareNulla()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));
        var schema = "{\"type\":\"object\",\"description\":\"" + new string('x', 6_000) + "\"}";
        var fornitore = new FornitoreClaudeCode(
            new ImpostazioniClaudeCode(finto.Eseguibile, "m", TimeSpan.FromSeconds(60), MassimoRigaComando: 5_000), new ValidatoreSempre([]));

        var risposta = await fornitore.EstraiPaginaAsync(Richiesta() with { JsonSchema = schema });

        risposta.Esito.Should().Be(EsitoEstrazione.ErroreDefinitivo);
        risposta.Messaggio.Should().Contain("schema JSON").And.Contain("troppo lungo").And.Contain("5000");
        risposta.Chiamate.Should().BeEmpty();
        finto.Invocazioni.Should().BeEmpty();
    }

    [Fact]
    public async Task Immagine_ESuFileInUnaCartellaDedicataCheDopoLaChiamataSparisce()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));

        await Crea(finto).EstraiPaginaAsync(Richiesta());

        var invocazione = finto.Invocazioni.Single();
        invocazione.File.Should().Equal("pagina.png");
        invocazione.LunghezzaPng.Should().Be(Png.Length);
        Directory.Exists(invocazione.Cwd).Should().BeFalse();
        string.Join(' ', invocazione.Args).Should().Contain("Read");
    }

    [Fact]
    public async Task AmbienteDelFiglio_NonHaLeChiaviCheCambiavoAutenticazione()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(RispostaValida));
        var chiave = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        var token = Environment.GetEnvironmentVariable("ANTHROPIC_AUTH_TOKEN");
        var baseUrl = Environment.GetEnvironmentVariable("ANTHROPIC_BASE_URL");
        Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", "chiave-di-prova");
        Environment.SetEnvironmentVariable("ANTHROPIC_AUTH_TOKEN", "token-di-prova");
        Environment.SetEnvironmentVariable("ANTHROPIC_BASE_URL", "https://altro.invalid");
        try
        {
            await Crea(finto).EstraiPaginaAsync(Richiesta());
        }
        finally
        {
            Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", chiave);
            Environment.SetEnvironmentVariable("ANTHROPIC_AUTH_TOKEN", token);
            Environment.SetEnvironmentVariable("ANTHROPIC_BASE_URL", baseUrl);
        }

        var invocazione = finto.Invocazioni.Single();
        invocazione.ChiaveApi.Should().BeFalse();
        invocazione.TokenAuth.Should().BeFalse();
        invocazione.BaseUrl.Should().BeFalse();
    }

    [Theory]
    [InlineData("claude.cmd")]
    [InlineData("claude.cmd.")]
    [InlineData("claude.bat. ")]
    [InlineData("C:\\strumenti\\claude.BAT")]
    public async Task EseguibileCmdOBat_ERifiutatoSenzaAvviareNulla(string eseguibile)
    {
        var fornitore = new FornitoreClaudeCode(
            new ImpostazioniClaudeCode(eseguibile, "m", TimeSpan.FromSeconds(5)), new ValidatoreSempre([]));

        var risposta = await fornitore.EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.ErroreDefinitivo);
        risposta.Messaggio.Should().Contain("nativo");
    }

    [Fact]
    public async Task EseguibileInesistente_EErroreDefinitivo()
    {
        var inesistente = Path.Combine(Path.GetTempPath(), "inesistente", "claude.exe");
        var fornitore = new FornitoreClaudeCode(
            new ImpostazioniClaudeCode(inesistente, "m", TimeSpan.FromSeconds(5)), new ValidatoreSempre([]));

        var risposta = await fornitore.EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.ErroreDefinitivo);
    }

    [Fact]
    public async Task Annullamento_LanciaOperationCanceledEChiudeIlProcesso()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto("", SleepMs: 60_000));
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        var chiamata = () => Crea(finto).EstraiPaginaAsync(Richiesta(), cts.Token);

        await chiamata.Should().ThrowAsync<OperationCanceledException>();
        foreach (var invocazione in finto.Invocazioni)
            ProcessoTerminato(invocazione.Pid).Should().BeTrue();
    }

    private static bool ProcessoTerminato(int pid)
    {
        for (var i = 0; i < 20; i++)
        {
            try
            {
                using var processo = Process.GetProcessById(pid);
                if (processo.HasExited)
                    return true;
            }
            catch (ArgumentException)
            {
                return true;
            }

            Thread.Sleep(100);
        }

        return false;
    }

    private sealed class ValidatoreSempre(IReadOnlyList<string> errori) : IValidatoreSchemaJson
    {
        public IReadOnlyList<string> Valida(string json, string jsonSchema) => errori;
    }

    private sealed class ValidatoreSequenza(IReadOnlyList<string>[] sequenza) : IValidatoreSchemaJson
    {
        private int _indice;

        public IReadOnlyList<string> Valida(string json, string jsonSchema) =>
            sequenza[Math.Min(_indice++, sequenza.Length - 1)];
    }

    private sealed class TempoFisso(DateTimeOffset adesso) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => adesso;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
