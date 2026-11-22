using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Valutazione;
using FluentAssertions;
using static DBEnigmistica.Tests.Unit.Valutazione.ServiziDiProva;

namespace DBEnigmistica.Tests.Unit.Valutazione;

public sealed class RegressioneEstrazioneTests : IDisposable
{
    private readonly string _radice = Path.Combine(Path.GetTempPath(), "dbe-regressione-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_radice))
            Directory.Delete(_radice, recursive: true);
    }

    // Copia del Set sintetico in una cartella temporanea, da alterare nei test.
    private string CopiaSet()
    {
        var destinazione = Path.Combine(_radice, "set");
        CopiaCartella(SetVersionato, destinazione);
        return destinazione;
    }

    [Fact]
    public async Task Replay_SulSetSintetico_LeggeSeiPagineConZeroChiamate()
    {
        var risultato = await Servizio().EseguiAsync(Parametri(SetVersionato), Riproduci(SetVersionato));

        risultato.ChiamateAi.Should().Be(0);
        risultato.Rapporto.Pagine.Should().HaveCount(6);
        risultato.Rapporto.Pagine.Should().OnlyContain(p => p.RispostaValida, "le risposte versionate devono rispettare lo schema derivato dal catalogo");
        var totale = risultato.Rapporto.Gruppi["totale"];
        totale.Pagine.Should().Be(6);
        totale.Richiamo.Should().Be(1.0);
        totale.Precisione.Should().BeLessThan(1.0, "una Registrazione contiene un Gioco in più voluto");
        risultato.Rapporto.Gruppi["scansione"].Pagine.Should().Be(1);
    }

    [Fact]
    public async Task Replay_SulSetSintetico_LaLineaDiBaseNonETuttaAl100()
    {
        var risultato = await Servizio().EseguiAsync(Parametri(SetVersionato), Riproduci(SetVersionato));

        var totale = risultato.Rapporto.Gruppi["totale"];
        new[] { totale.Precisione, totale.CaselleCorrette, totale.CampiEsatti }.Should().OnlyContain(v => v < 1.0);
        totale.Cer.Should().BeGreaterThan(0.0);
        risultato.ConfrontoDisponibile.Should().BeTrue();
        risultato.Regressioni.Should().BeEmpty();
    }

    [Fact]
    public async Task Replay_DueEsecuzioni_ProduconoLoStessoRapporto()
    {
        var primo = await Servizio().EseguiAsync(Parametri(SetVersionato), Riproduci(SetVersionato));
        var secondo = await Servizio().EseguiAsync(Parametri(SetVersionato), Riproduci(SetVersionato));

        secondo.Json.Should().Be(primo.Json);
        secondo.Markdown.Should().Be(primo.Markdown);
    }

    [Fact]
    public async Task Replay_SenzaLineaDiBase_SegnalaCheIlConfrontoManca()
    {
        var cartella = CopiaSet();
        File.Delete(Path.Combine(cartella, "linea-di-base.json"));

        var risultato = await Servizio().EseguiAsync(Parametri(cartella), Riproduci(cartella));

        risultato.ConfrontoDisponibile.Should().BeFalse();
        risultato.Regressioni.Should().BeEmpty();
        risultato.Markdown.Should().Contain("Linea di base assente");
    }

    [Fact]
    public async Task Replay_PaginaSenzaRegistrazione_ErroreEsplicitoEZeroChiamate()
    {
        var cartella = CopiaSet();
        var file = Directory.GetFiles(Path.Combine(cartella, "registrazioni"), "*.json", SearchOption.AllDirectories).Order().First();
        File.Delete(file);
        var spia = new FornitoreSpia(Riproduci(cartella));

        var azione = () => Servizio().EseguiAsync(Parametri(cartella), spia);

        await azione.Should().ThrowAsync<ValutazioneException>().WithMessage("*Registrazione mancante*");
        spia.ChiamateReali.Should().Be(0);
    }

    [Fact]
    public async Task Replay_ImprontaDelloAttesoDiversa_ErroreCheDiceLaPagina()
    {
        var cartella = CopiaSet();
        var percorso = Path.Combine(cartella, "attesi", "s02.json");
        var testo = await File.ReadAllTextAsync(percorso);
        var impronta = System.Text.RegularExpressions.Regex.Match(testo, "\"impronta\": \"([0-9a-f]{64})\"").Groups[1].Value;
        await File.WriteAllTextAsync(percorso, testo.Replace(impronta, new string('0', 64), StringComparison.Ordinal));

        var azione = () => Servizio().EseguiAsync(Parametri(cartella), Riproduci(cartella));

        await azione.Should().ThrowAsync<ValutazioneException>().WithMessage("*s02*impronta*");
    }

    [Fact]
    public async Task Replay_PaginaInBozza_EsclusaDalleMetricheEElencata()
    {
        var cartella = CopiaSet();
        File.Delete(Path.Combine(cartella, "linea-di-base.json"));
        var percorso = Path.Combine(cartella, "attesi", "s05.json");
        var testo = await File.ReadAllTextAsync(percorso);
        await File.WriteAllTextAsync(percorso, testo.Replace("\"stato\": \"corretto\"", "\"stato\": \"bozza\"", StringComparison.Ordinal));

        var risultato = await Servizio().EseguiAsync(Parametri(cartella), Riproduci(cartella));

        risultato.Rapporto.Gruppi["totale"].Pagine.Should().Be(5);
        risultato.Rapporto.Pagine.Single(p => p.Id == "s05").InBozza.Should().BeTrue();
        risultato.Markdown.Should().Contain("s05");
    }

    [Fact]
    public async Task Replay_RispostaNonConformeAlloSchema_PaginaNonValidaEGiochiMancanti()
    {
        var cartella = CopiaSet();
        File.Delete(Path.Combine(cartella, "linea-di-base.json"));
        await File.WriteAllTextAsync(Path.Combine(cartella, "risposte", "s02.json"), "{\"versione_formato\":1,\"giochi\":[{\"x\":1}]}");
        await Produttore().AggiornaAsync(cartella, "finto", "finto-1", 200, scriviLineaDiBase: false);

        var risultato = await Servizio().EseguiAsync(Parametri(cartella), Riproduci(cartella));

        var pagina = risultato.Rapporto.Pagine.Single(p => p.Id == "s02");
        pagina.RispostaValida.Should().BeFalse();
        pagina.Mancanti.Should().Be(3);
        pagina.Estratti.Should().Be(0);
    }

    [Fact]
    public async Task AggiornaChiavi_SuSetDiRiferimento_Rifiutato()
    {
        var cartella = CopiaSet();
        var percorso = Path.Combine(cartella, "set.json");
        var testo = await File.ReadAllTextAsync(percorso);
        await File.WriteAllTextAsync(percorso, testo.Replace("\"tipo\": \"sintetico\"", "\"tipo\": \"riferimento\"", StringComparison.Ordinal)
            .Replace("\"generatore\": \"set-sintetico-v1\",", string.Empty, StringComparison.Ordinal)
            .Replace("\"pdf\": null", "\"pdf\": \"pdf/riviste.pdf\"", StringComparison.Ordinal));

        var azione = () => Produttore().AggiornaAsync(cartella, "finto", "finto-1", 200);

        await azione.Should().ThrowAsync<SetNonValidoException>().WithMessage("*riferimento*");
    }

    [Fact]
    public async Task AggiornaChiavi_SulSetSintetico_RiproduceEsattamenteRegistrazioniELineaDiBaseVersionate()
    {
        var cartella = CopiaSet();

        await Produttore().AggiornaAsync(cartella, "finto", "finto-1", 200);

        Elenco(cartella).Should().Equal(Elenco(SetVersionato), "i file versionati devono essere quelli che il programma produce dalle risposte");
    }

    private static IEnumerable<(string Percorso, string Contenuto)> Elenco(string radice) =>
        Directory.GetFiles(Path.Combine(radice, "registrazioni"), "*.json", SearchOption.AllDirectories)
            .Append(Path.Combine(radice, "linea-di-base.json"))
            .Select(f => (Path.GetRelativePath(radice, f).Replace('\\', '/'), File.ReadAllText(f)))
            .Order();

    // Conta le chiamate reali che le risposte dichiarano di aver fatto.
    private sealed class FornitoreSpia(IFornitoreAiEstrazione interno) : IFornitoreAiEstrazione
    {
        public int ChiamateReali { get; private set; }

        public string IdFornitore => interno.IdFornitore;

        public string Modello => interno.Modello;

        public async Task<RispostaEstrazionePagina> EstraiPaginaAsync(RichiestaEstrazionePagina richiesta, CancellationToken cancellationToken = default)
        {
            var risposta = await interno.EstraiPaginaAsync(richiesta, cancellationToken);
            ChiamateReali += risposta.Chiamate.Count;
            return risposta;
        }
    }
}
