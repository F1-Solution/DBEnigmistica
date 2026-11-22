using System.Text.Json;
using DBEnigmistica.Application.Valutazione;
using DBEnigmistica.Tests.Unit.Creazione;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Modello;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Valutazione;

public class ConfrontoDatiTests
{
    private static readonly IReadOnlyList<Descrittore> Catalogo =
        new CaricatoreDescrittori().CaricaCartella(FileDelRepository.Percorso("tipologie")).PerEstrazione();

    private static Descrittore D(string id) => Catalogo.Single(d => d.Id == id);

    private static GiocoPagina G(string tipologia, string datiJson, params string[] nonValutati) =>
        new("nota:" + tipologia, new RiquadroGioco(0, 0, 1, 1), JsonDocument.Parse(datiJson).RootElement.Clone(), nonValutati);

    private static string Griglia(int righe, int colonne, string[] celle, params (int Riga, int Colonna, int Numero)[] numeri) =>
        $$"""
        { "righe": {{righe}}, "colonne": {{colonne}}, "celle": [{{string.Join(",", celle.Select(c => $"\"{c}\""))}}],
          "numeri": [{{string.Join(",", numeri.Select(n => $"{{ \"riga\": {n.Riga}, \"colonna\": {n.Colonna}, \"numero\": {n.Numero} }}"))}}] }
        """;

    private static GiocoPagina Cruciverba(string griglia, string definizioni = "[]", params string[] nonValutati) =>
        G("cruciverba-classico", $$"""{ "righe": 2, "colonne": 2, "griglia": {{griglia}}, "autore": null, "definizioni": {{definizioni}} }""", nonValutati);

    private static string Voce(int numero, string direzione, string? testo) =>
        $$"""{ "numero": {{numero}}, "direzione": "{{direzione}}", "testo": {{(testo is null ? "null" : JsonSerializer.Serialize(testo))}}, "soluzione": null }""";

    private static DettaglioCoppia Confronta(string id, GiocoPagina atteso, GiocoPagina estratto) =>
        ConfrontoDati.Confronta(D(id), atteso, estratto);

    // ---- Normalizzazione e distanza ----

    [Theory]
    [InlineData("L’Opposto  di sotto", "l'opposto di sotto")]
    [InlineData("  «Ciao»  ", "\"ciao\"")]
    [InlineData("a – b — c", "a - b - c")]
    [InlineData("così…", "così...")]
    [InlineData("l' albero", "l'albero")]
    [InlineData("l ' albero", "l'albero")]
    [InlineData("uno due tre", "uno due tre")]
    public void Normalizza_ApplicaLeRegoleDellaSezione43(string testo, string atteso)
    {
        NormalizzazioneTesto.Normalizza(testo).Should().Be(atteso);
    }

    [Fact]
    public void Distanza_ContaIcaratteriUnicodeNonLeUnitaUtf16()
    {
        NormalizzazioneTesto.Distanza("a😀b", "ab").Should().Be(1);
        NormalizzazioneTesto.Lunghezza("a😀b").Should().Be(3);
    }

    [Fact]
    public void Distanza_IgnoraMaiuscoleEApostrofiTipografici()
    {
        NormalizzazioneTesto.Distanza("L’Opposto DI sotto", "l'opposto di SOTTO").Should().Be(0);
    }

    // ---- CER ----

    [Fact]
    public void Cer_DefinizioneConMaiuscoleEApostrofiDiversi_NessunErrore()
    {
        var a = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Capitale d'Italia")}]");
        var e = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "CAPITALE D’ITALIA")}]");

        var dettaglio = Confronta("cruciverba-classico", a, e);

        dettaglio.CerErrori.Should().Be(0);
        dettaglio.CerLunghezza.Should().Be(17);
    }

    [Fact]
    public void Cer_DefinizioneSbagliata_ContaLaDistanzaESulLunghezzaAttesa()
    {
        var a = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Capitale d'Italia")}]");
        var e = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Capitale di Italia")}]");

        var dettaglio = Confronta("cruciverba-classico", a, e);

        dettaglio.CerErrori.Should().Be(2);
        dettaglio.CerLunghezza.Should().Be(17);
    }

    [Fact]
    public void Cer_VoceAttesaSenzaVoceEstratta_ContaLaLunghezzaAttesa()
    {
        var a = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Uno")},{Voce(2, "verticale", "Quattro")}]");
        var e = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Uno")}]");

        var dettaglio = Confronta("cruciverba-classico", a, e);

        dettaglio.CerErrori.Should().Be(7);
        dettaglio.CerLunghezza.Should().Be(10);
    }

    [Fact]
    public void Cer_VoceEstrattaInPiu_ContaLaSuaLunghezzaSenzaCambiareIlDenominatore()
    {
        var a = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Uno")}]");
        var e = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Uno")},{Voce(2, "verticale", "Quattro")}]");

        var dettaglio = Confronta("cruciverba-classico", a, e);

        dettaglio.CerErrori.Should().Be(7);
        dettaglio.CerLunghezza.Should().Be(3);
    }

    [Fact]
    public void Cer_LeVociSiAllineanoPerNumeroEDirezioneNonPerPosizione()
    {
        var a = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Uno")},{Voce(1, "verticale", "Due")}]");
        var e = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "verticale", "Due")},{Voce(1, "orizzontale", "Uno")}]");

        Confronta("cruciverba-classico", a, e).CerErrori.Should().Be(0);
    }

    [Fact]
    public void Cer_AttesoNulloEdEstrattoTesto_ContaLaLunghezzaEstratta_AttesoNulloEdEstrattoNulloNonConta()
    {
        var nulli = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", null)}]");
        var testo = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Tre")}]");

        var entrambiNulli = Confronta("cruciverba-classico", nulli, nulli);
        var soloEstratto = Confronta("cruciverba-classico", nulli, testo);

        entrambiNulli.CerErrori.Should().Be(0);
        entrambiNulli.CerLunghezza.Should().Be(0);
        soloEstratto.CerErrori.Should().Be(3);
        soloEstratto.CerLunghezza.Should().Be(0);
    }

    [Fact]
    public void Cer_IlCampoEspostoDiUnGiocoBreveConta()
    {
        var a = Anagramma("Il cielo di notte");
        var e = Anagramma("Il cielo di notti");

        var dettaglio = Confronta("anagramma", a, e);

        dettaglio.CerErrori.Should().Be(1);
        dettaglio.CerLunghezza.Should().Be(17);
    }

    [Fact]
    public void Cer_UnElencoNonValutatoResta_FuoriDalConteggio()
    {
        var a = Cruciverba(Griglia(1, 1, ["."]), $"[{Voce(1, "orizzontale", "Uno")}]", "definizioni");
        var e = Cruciverba(Griglia(1, 1, ["."]), "[]");

        var dettaglio = Confronta("cruciverba-classico", a, e);

        dettaglio.CerErrori.Should().Be(0);
        dettaglio.CerLunghezza.Should().Be(0);
    }

    // ---- Caselle ----

    [Fact]
    public void Caselle_GrigliaUguale_TutteCorrette()
    {
        var g = Griglia(2, 2, [".#", ".."], (1, 1, 1));

        var dettaglio = Confronta("cruciverba-classico", Cruciverba(g), Cruciverba(g));

        dettaglio.CaselleCorrette.Should().Be(4);
        dettaglio.CaselleAttese.Should().Be(4);
    }

    [Fact]
    public void Caselle_CategoriaSbagliata_UnaCasellaErrata()
    {
        var a = Cruciverba(Griglia(2, 2, [".#", ".."], (1, 1, 1)));
        var e = Cruciverba(Griglia(2, 2, ["..", ".."], (1, 1, 1)));

        var dettaglio = Confronta("cruciverba-classico", a, e);

        dettaglio.CaselleCorrette.Should().Be(3);
        dettaglio.CaselleAttese.Should().Be(4);
    }

    [Fact]
    public void Caselle_NumeroMancanteODiverso_CasellaErrata()
    {
        var a = Cruciverba(Griglia(2, 2, [".#", ".."], (1, 1, 1), (2, 1, 2)));
        var e = Cruciverba(Griglia(2, 2, [".#", ".."], (1, 1, 1), (2, 1, 3)));

        Confronta("cruciverba-classico", a, e).CaselleCorrette.Should().Be(3);
    }

    [Fact]
    public void Caselle_PuntoInterrogativoNellEstrattoEErrato_NellAttesoEscludeLaCasella()
    {
        var a = Cruciverba(Griglia(2, 2, [".#", ".?"]));
        var e = Cruciverba(Griglia(2, 2, ["?#", ".."]));

        var dettaglio = Confronta("cruciverba-classico", a, e);

        dettaglio.CaselleAttese.Should().Be(3);
        dettaglio.CaselleCorrette.Should().Be(2);
    }

    [Fact]
    public void Caselle_UnaLetteraEUnPuntoSonoLaStessaCategoria()
    {
        var a = Cruciverba(Griglia(2, 2, [".#", ".."]));
        var e = Cruciverba(Griglia(2, 2, ["A#", "BC"]));

        Confronta("cruciverba-classico", a, e).CaselleCorrette.Should().Be(4);
    }

    [Fact]
    public void Caselle_DimensioniDiverse_TutteLeCaselleAtteseSonoErrate()
    {
        var a = Cruciverba(Griglia(2, 2, [".#", ".."]));
        var e = Cruciverba(Griglia(3, 2, [".#", "..", ".."]));

        var dettaglio = Confronta("cruciverba-classico", a, e);

        dettaglio.CaselleCorrette.Should().Be(0);
        dettaglio.CaselleAttese.Should().Be(4);
    }

    [Fact]
    public void Caselle_GrigliaNonValutata_FuoriDalConteggio()
    {
        var a = Cruciverba(Griglia(2, 2, [".#", ".."]), "[]", "griglia");
        var e = Cruciverba(Griglia(2, 2, ["..", ".."]));

        Confronta("cruciverba-classico", a, e).CaselleAttese.Should().Be(0);
    }

    // ---- Campi esatti ----

    private static GiocoPagina Anagramma(string esposto, string diagramma = "5 = 5", string? soluzione = null, string? autore = null, params string[] nonValutati) =>
        G("anagramma", $$"""
            { "esposto": {{JsonSerializer.Serialize(esposto)}}, "diagramma": {{JsonSerializer.Serialize(diagramma)}}, "prima_parola": null,
              "soluzione": {{(soluzione is null ? "null" : JsonSerializer.Serialize(soluzione))}}, "autore": {{(autore is null ? "null" : JsonSerializer.Serialize(autore))}} }
            """, nonValutati);

    [Fact]
    public void CampiEsatti_GiocoUguale_TuttiEsatti()
    {
        var dettaglio = Confronta("anagramma", Anagramma("Il cielo"), Anagramma("Il cielo"));

        dettaglio.CampiConfrontati.Should().Be(5);
        dettaglio.CampiUguali.Should().Be(5);
    }

    [Fact]
    public void CampiEsatti_TestiUguali_DopoLaNormalizzazione()
    {
        var dettaglio = Confronta("anagramma", Anagramma("L'albero", autore: "Anna  Prova"), Anagramma("l’ALBERO", autore: "anna prova"));

        dettaglio.CampiUguali.Should().Be(5);
    }

    [Fact]
    public void CampiEsatti_UnCampoDiverso_NonContaTraGliEsatti()
    {
        var dettaglio = Confronta("anagramma", Anagramma("Il cielo", soluzione: "Cenere"), Anagramma("Il cielo", soluzione: "Cenera"));

        dettaglio.CampiConfrontati.Should().Be(5);
        dettaglio.CampiUguali.Should().Be(4);
    }

    [Fact]
    public void CampiEsatti_NullContraNullEUguale_NullContraTestoNo()
    {
        var dettaglio = Confronta("anagramma", Anagramma("Il cielo"), Anagramma("Il cielo", autore: "Inventato"));

        dettaglio.CampiUguali.Should().Be(4);
    }

    [Fact]
    public void CampiEsatti_UnCampoNonValutato_RestaFuoriDalConfronto()
    {
        var dettaglio = Confronta("anagramma", Anagramma("Il cielo", nonValutati: "autore"), Anagramma("Il cielo", autore: "Inventato"));

        dettaglio.CampiConfrontati.Should().Be(4);
        dettaglio.CampiUguali.Should().Be(4);
    }

    [Fact]
    public void CampiEsatti_LaFamigliaCruciverbaNonContaIcampiEsatti()
    {
        var g = Griglia(1, 1, ["."]);

        var dettaglio = Confronta("cruciverba-classico", Cruciverba(g), Cruciverba(g));

        dettaglio.CampiConfrontati.Should().Be(0);
    }

    // ---- Somma dei gruppi ----

    [Fact]
    public void Somma_ConDenominatoreZero_LaMetricaENulla_MaiCento()
    {
        var pagina = ConfrontoPagina.Calcola([], [], new Dictionary<string, Descrittore>());

        var gruppo = CalcoloMetriche.Somma([pagina]);

        gruppo.Richiamo.Should().BeNull();
        gruppo.Precisione.Should().BeNull();
        gruppo.TipologiaCorretta.Should().BeNull();
        gruppo.CaselleCorrette.Should().BeNull();
        gruppo.Cer.Should().BeNull();
        gruppo.CampiEsatti.Should().BeNull();
    }

    [Fact]
    public void Somma_ESommaDiNumeratoriEDenominatori_NonMediaDiRapporti()
    {
        var descrittori = Catalogo.ToDictionary(d => d.Id);
        var g = Griglia(2, 2, [".#", ".."]);
        // Pagina 1: 4 caselle su 4. Pagina 2: 2 su 4. Micro-media: 6 su 8.
        var p1 = ConfrontoPagina.Calcola([Cruciverba(g)], [Cruciverba(g)], descrittori);
        var p2 = ConfrontoPagina.Calcola([Cruciverba(g)], [Cruciverba(Griglia(2, 2, ["..", "#."]))], descrittori);

        CalcoloMetriche.Somma([p1, p2]).CaselleCorrette.Should().Be(0.75);
    }

    [Fact]
    public void Arrotondamento_QuattroDecimaliLontanoDaZero()
    {
        CalcoloMetriche.Arrotonda(0.12345).Should().Be(0.1235);
        CalcoloMetriche.Arrotonda(2.0 / 3.0).Should().Be(0.6667);
    }
}
