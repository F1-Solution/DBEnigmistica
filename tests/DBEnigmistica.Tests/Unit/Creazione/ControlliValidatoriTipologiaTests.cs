using System.Text.Json.Nodes;
using System.Text.Json;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Modello;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Creazione;

/// <summary>Controlli leggeri dei validatori della 1.2 sul contenuto di un candidato: solo informazioni per il revisore (R2, R3).</summary>
public sealed class ControlliValidatoriTipologiaTests
{
    private static JsonNode Risposta(string nome) => JsonNode.Parse(FileDelRepository.Esempio("risposta", nome))!;

    [Fact]
    public void Anagramma_ConLettereDiverse_DaUnControlloEQuelloCorrettoNo()
    {
        var corretto = Risposta("valido-esempio-anagramma.json");
        corretto["dati"]!["prima_parola"] = "mare";
        corretto["dati"]!["soluzione"] = "rame";
        var sbagliato = JsonNode.Parse(corretto.ToJsonString())!;
        sbagliato["dati"]!["soluzione"] = "ramo";

        ControlliValidatoriTipologia.Verifica("anagramma", 1, corretto.ToJsonString()).Should().BeEmpty();
        ControlliValidatoriTipologia.Verifica("anagramma", 1, sbagliato.ToJsonString())
            .Should().Contain(m => m.Contains("soluzione", StringComparison.Ordinal) && m.Contains("stesse lettere", StringComparison.Ordinal));
    }

    [Fact]
    public void Crittografia_ConDiagrammaIncoerente_DaUnControllo()
    {
        var corretta = Risposta("valido-crittografia-sinonimica.json");
        var incoerente = Risposta("valido-crittografia-sinonimica.json");
        incoerente["dati"]!["diagramma"] = "3 3 = 9";

        ControlliValidatoriTipologia.Verifica("crittografia-breve", 0, corretta.ToJsonString()).Should().BeEmpty();
        ControlliValidatoriTipologia.Verifica("crittografia-breve", 0, incoerente.ToJsonString()).Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("valido-sudoku.json", "sudoku", 0)]
    [InlineData("valido-futoshiki-5x5.json", "futoshiki", 0)]
    [InlineData("valido-nonogramma-10x10.json", "nonogramma", 0)]
    [InlineData("valido-esempio-cruciverba-9x9.json", "cruciverba-classico", 1)]
    public void GliEsempiValidi_NonDannoControlli(string file, string tipologia, int versione)
    {
        ControlliValidatoriTipologia.Verifica(tipologia, versione, Risposta(file).ToJsonString()).Should().BeEmpty();
    }

    [Fact]
    public void Rebus_ConDiagrammaDelleLunghezzeDelleParole_NonDaControlli()
    {
        ControlliValidatoriTipologia.Verifica("rebus", 0, Risposta("valido-rebus.json").ToJsonString()).Should().BeEmpty();
    }

    [Fact]
    public void Rebus_ConLunghezzeCheNonCorrispondonoAllaSoluzione_DaUnControllo()
    {
        var sbagliato = Risposta("valido-rebus.json");
        sbagliato["dati"]!["diagramma"] = "6 5";

        ControlliValidatoriTipologia.Verifica("rebus", 0, sbagliato.ToJsonString()).Should().ContainSingle().Which.Should().Contain("diagramma");
    }

    private sealed class ValidatoreCheLancia : IValidatoreStrutturale
    {
        public NomeValidatore Nome => NomeValidatore.AnagrammaStesseLettere;

        public IReadOnlyList<RuoloCampo> RuoliRichiesti => [];

        public EsitoValidazione Valida(Descrittore descrittore, JsonElement dati) => throw new NullReferenceException("guasto di prova");
    }

    [Fact]
    public void UnValidatoreCheLanciaQualsiasiEccezione_DaUnMessaggioENonLancia()
    {
        var contenuto = Risposta("valido-esempio-anagramma.json").ToJsonString();

        var messaggi = ControlliValidatoriTipologia.Verifica("anagramma", 1, contenuto, [new ValidatoreCheLancia()]);

        messaggi.Should().ContainSingle().Which.Should().Contain("non si può eseguire");
    }

    [Fact]
    public void UnaCaricamentoFallito_NonResta_InCache()
    {
        var chiamate = 0;
        var fornitore = new ValoreRiprovabile<int>(() => ++chiamate == 1 ? throw new InvalidOperationException("primo tentativo") : 42);

        var primo = () => fornitore.Valore;
        primo.Should().Throw<InvalidOperationException>();

        fornitore.Valore.Should().Be(42);
        fornitore.Valore.Should().Be(42);
        chiamate.Should().Be(2);
    }

    [Fact]
    public void ConIlCatalogoNonCaricabile_NonLanciaEDaUnMessaggio()
    {
        var messaggi = ControlliValidatoriTipologia.Verifica(
            "anagramma", 1, "{}", new ValoreRiprovabile<CatalogoTipologie>(() => throw new IOException("risorsa mancante")));

        messaggi.Should().ContainSingle().Which.Should().Contain("non si possono eseguire");
    }

    [Fact]
    public void TipologiaOVersioneSconosciuta_NonDaControlli()
    {
        ControlliValidatoriTipologia.Verifica("inesistente", 0, "{}").Should().BeEmpty();
    }

    [Fact]
    public void ContenutoSenzaDati_DaUnMessaggioENonLancia()
    {
        ControlliValidatoriTipologia.Verifica("anagramma", 1, "{\"titolo\":null}").Should().NotBeEmpty();
    }
}
