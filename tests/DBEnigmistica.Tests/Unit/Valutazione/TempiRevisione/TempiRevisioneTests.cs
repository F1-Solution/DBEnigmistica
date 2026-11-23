using System.Text.Json;
using DBEnigmistica.Application.Valutazione.TempiRevisione;
using DBEnigmistica.Tests.Unit.Creazione;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Valutazione.TempiRevisione;

/// <summary>Tempo attivo e rapporto <c>review-time</c> (progetto 1b.1, sezioni 8.1 e 8.2); i valori attesi sono calcolati a mano.</summary>
public sealed class TempiRevisioneTests
{
    private static readonly DateTimeOffset Adesso = new(2026, 12, 2, 18, 0, 0, TimeSpan.Zero);

    private static IReadOnlyList<CandidatoPerRapporto> Sintetici() =>
        LettoreCandidatiDaFile.Leggi(File.ReadAllText(FileDelRepository.Percorso("tests", "DBEnigmistica.Tests", "Fixtures", "CandidatiSintetici", "candidati.json")));

    private static JsonElement Gruppo(JsonElement rapporto, string nome) =>
        rapporto.GetProperty("gruppi"u8).EnumerateArray().Single(g => g.GetProperty("gruppo"u8).GetString() == nome);

    private static JsonElement Json(IReadOnlyList<CandidatoPerRapporto> candidati) =>
        JsonDocument.Parse(CalcolatoreTempiRevisione.Calcola(candidati, Adesso).InJson()).RootElement;

    // Tempo attivo (8.1).

    [Fact]
    public void TempoAttivo_EsempioDelProgetto_Da160Secondi()
    {
        // Istanti in secondi: 0, 60, 150, ricaricamento, 400, 410: intervalli 60, 90, 250, 10.
        CalcoloTempoAttivo.Da([0, 60_000, 150_000, 400_000, 410_000]).Should().Be(160_000);
    }

    [Theory]
    [InlineData(120_000, 120_000)]
    [InlineData(120_001, 0)]
    [InlineData(-5_000, 0)]
    public void TempoAttivo_UnSoloIntervallo(long intervallo, long atteso)
    {
        CalcoloTempoAttivo.Da([1_000_000, 1_000_000 + intervallo]).Should().Be(atteso);
    }

    [Fact]
    public void TempoAttivo_SenzaIstantiOConUnSoloIstante_Zero()
    {
        CalcoloTempoAttivo.Da([]).Should().Be(0);
        CalcoloTempoAttivo.Da([5_000]).Should().Be(0);
    }

    // Quantili (metodo 7).

    [Theory]
    [InlineData(0.5, 12.5)]
    [InlineData(0.25, 8.75)]
    [InlineData(0.75, 17.5)]
    public void Quantile_InterpolaLinearmente(double p, double atteso)
    {
        Quantili.Calcola([5, 10, 15, 25], p).Should().Be(atteso);
    }

    [Fact]
    public void Quantile_ConUnSoloValore_ESempreQuelloEConNessunoENull()
    {
        Quantili.Calcola([7.5], 0.25).Should().Be(7.5);
        Quantili.Calcola([], 0.5).Should().BeNull();
    }

    // Rapporto sui 30 candidati sintetici.

    [Fact]
    public void Rapporto_HaISeiGruppiInOrdineFisso()
    {
        var r = Json(Sintetici());

        r.GetProperty("versione_formato"u8).GetInt32().Should().Be(1);
        r.GetProperty("generato_il"u8).GetString().Should().Be("2026-12-02T18:00:00Z");
        r.GetProperty("gruppi"u8).EnumerateArray().Select(g => g.GetProperty("gruppo"u8).GetString())
            .Should().Equal("crittografia_breve", "anagramma", "cruciverba", "rebus", "sudoku", "giochi_logici");
    }

    [Fact]
    public void Crittografia_ConPausaOltreDueMinutiEMedianaInterpolata()
    {
        var g = Gruppo(Json(Sintetici()), "crittografia_breve");

        g.GetProperty("tipologie"u8).EnumerateArray().Single().GetProperty("tipologia"u8).GetString().Should().Be("crittografia-breve");
        g.GetProperty("tipologie"u8)[0].GetProperty("versioni"u8).EnumerateArray().Select(v => v.GetInt32()).Should().Equal(0);
        g.GetProperty("generati"u8).GetInt32().Should().Be(7);
        g.GetProperty("validi"u8).GetInt32().Should().Be(6);
        g.GetProperty("non_validi_schema"u8).GetInt32().Should().Be(1);
        g.GetProperty("errori_generazione"u8).GetInt32().Should().Be(1);
        g.GetProperty("quota_non_validi"u8).GetDouble().Should().Be(0.1429);
        g.GetProperty("rivisti"u8).GetInt32().Should().Be(4);
        g.GetProperty("non_rivisti"u8).GetInt32().Should().Be(2);
        g.GetProperty("accettati"u8).GetInt32().Should().Be(2);
        g.GetProperty("scartati"u8).GetInt32().Should().Be(2);
        g.GetProperty("quota_accettata"u8).GetDouble().Should().Be(0.5);
        var m = g.GetProperty("minuti"u8);
        m.GetProperty("mediana"u8).GetDouble().Should().Be(12.5);
        m.GetProperty("q1"u8).GetDouble().Should().Be(8.75);
        m.GetProperty("q3"u8).GetDouble().Should().Be(17.5);
        m.GetProperty("minimo"u8).GetDouble().Should().Be(5);
        m.GetProperty("massimo"u8).GetDouble().Should().Be(25);
        g.GetProperty("misura_indicativa"u8).GetBoolean().Should().BeTrue();
        var soglia = g.GetProperty("soglia_g1"u8);
        soglia.GetProperty("minuti_massimi"u8).GetInt32().Should().Be(20);
        soglia.GetProperty("quota_minima"u8).GetDouble().Should().Be(0.5);
        soglia.GetProperty("tempo_entro_soglia"u8).GetBoolean().Should().BeTrue();
        soglia.GetProperty("quota_entro_soglia"u8).GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Anagramma_ConUnSoloRivisto_TuttiIQuantiliValgonoQuelValore()
    {
        var g = Gruppo(Json(Sintetici()), "anagramma");

        g.GetProperty("rivisti"u8).GetInt32().Should().Be(1);
        var m = g.GetProperty("minuti"u8);
        new[] { "mediana", "q1", "q3", "minimo", "massimo" }.Select(k => m.GetProperty(k).GetDouble()).Should().OnlyContain(x => x == 25.0);
        g.GetProperty("quota_accettata"u8).GetDouble().Should().Be(1.0);
        g.GetProperty("misura_indicativa"u8).GetBoolean().Should().BeTrue();
        g.GetProperty("soglia_g1"u8).GetProperty("tempo_entro_soglia"u8).GetBoolean().Should().BeFalse();
        g.GetProperty("soglia_g1"u8).GetProperty("quota_entro_soglia"u8).GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Cruciverba_SenzaRivisti_DaNullEMaiZero()
    {
        var g = Gruppo(Json(Sintetici()), "cruciverba");

        g.GetProperty("generati"u8).GetInt32().Should().Be(3);
        g.GetProperty("validi"u8).GetInt32().Should().Be(2);
        g.GetProperty("rivisti"u8).GetInt32().Should().Be(0);
        g.GetProperty("non_rivisti"u8).GetInt32().Should().Be(2);
        g.GetProperty("quota_non_validi"u8).GetDouble().Should().Be(0.3333);
        g.GetProperty("quota_accettata"u8).ValueKind.Should().Be(JsonValueKind.Null);
        foreach (var chiave in new[] { "mediana", "q1", "q3", "minimo", "massimo" })
            g.GetProperty("minuti"u8).GetProperty(chiave).ValueKind.Should().Be(JsonValueKind.Null);
        g.GetProperty("misura_indicativa"u8).GetBoolean().Should().BeTrue();
        var soglia = g.GetProperty("soglia_g1"u8);
        soglia.GetProperty("minuti_massimi"u8).GetInt32().Should().Be(60);
        soglia.GetProperty("tempo_entro_soglia"u8).ValueKind.Should().Be(JsonValueKind.Null);
        soglia.GetProperty("quota_entro_soglia"u8).ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void Rebus_ConDodiciRivisti_NonEIndicativoESenzaSoglia()
    {
        var g = Gruppo(Json(Sintetici()), "rebus");

        g.GetProperty("rivisti"u8).GetInt32().Should().Be(12);
        g.GetProperty("accettati"u8).GetInt32().Should().Be(9);
        g.GetProperty("quota_accettata"u8).GetDouble().Should().Be(0.75);
        var m = g.GetProperty("minuti"u8);
        m.GetProperty("mediana"u8).GetDouble().Should().Be(6.5);
        m.GetProperty("q1"u8).GetDouble().Should().Be(3.75);
        m.GetProperty("q3"u8).GetDouble().Should().Be(9.25);
        m.GetProperty("minimo"u8).GetDouble().Should().Be(1);
        m.GetProperty("massimo"u8).GetDouble().Should().Be(12);
        g.GetProperty("misura_indicativa"u8).GetBoolean().Should().BeFalse();
        g.GetProperty("soglia_g1"u8).ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void Sudoku_ILaRevisioneInCorsoNonContaNeiTempi()
    {
        var g = Gruppo(Json(Sintetici()), "sudoku");

        g.GetProperty("validi"u8).GetInt32().Should().Be(4);
        g.GetProperty("rivisti"u8).GetInt32().Should().Be(3);
        g.GetProperty("non_rivisti"u8).GetInt32().Should().Be(1);
        g.GetProperty("quota_accettata"u8).GetDouble().Should().Be(0.6667);
        g.GetProperty("minuti"u8).GetProperty("mediana"u8).GetDouble().Should().Be(4);
        g.GetProperty("minuti"u8).GetProperty("q1"u8).GetDouble().Should().Be(3.5);
        g.GetProperty("minuti"u8).GetProperty("q3"u8).GetDouble().Should().Be(4.5);
        g.GetProperty("quota_non_validi"u8).GetDouble().Should().Be(0);
    }

    [Fact]
    public void GiochiLogici_RaggruppaFutoshikiENonogrammaEMettonoInsiemeAcettatiEScartati()
    {
        var g = Gruppo(Json(Sintetici()), "giochi_logici");

        g.GetProperty("tipologie"u8).EnumerateArray().Select(t => t.GetProperty("tipologia"u8).GetString()).Should().Equal("futoshiki", "nonogramma");
        g.GetProperty("rivisti"u8).GetInt32().Should().Be(2);
        g.GetProperty("minuti"u8).GetProperty("mediana"u8).GetDouble().Should().Be(15);
        g.GetProperty("minuti"u8).GetProperty("q1"u8).GetDouble().Should().Be(12.5);
        g.GetProperty("minuti"u8).GetProperty("q3"u8).GetDouble().Should().Be(17.5);
        g.GetProperty("quota_accettata"u8).GetDouble().Should().Be(0.5);
    }

    [Fact]
    public void NonValidiEErroriNonEntranoNeiTempiNellaQuota()
    {
        var g = Gruppo(Json(Sintetici()), "crittografia_breve");

        // Minimo 5 (lo scartato con la pausa), non 0: né i non validi né gli errori hanno tempi.
        g.GetProperty("minuti"u8).GetProperty("minimo"u8).GetDouble().Should().Be(5);
    }

    [Fact]
    public void Rapporto_StessiDatiStessoJsonSalvoGeneratoIl()
    {
        var a = CalcolatoreTempiRevisione.Calcola(Sintetici(), Adesso).InJson();
        var b = CalcolatoreTempiRevisione.Calcola(Sintetici().Reverse().ToList(), Adesso.AddHours(3)).InJson();

        b.Replace("21:00:00Z", "18:00:00Z", StringComparison.Ordinal).Should().Be(a);
    }

    [Fact]
    public void Rapporto_SenzaCandidati_HaIGruppiVuotiConNull()
    {
        var r = Json([]);

        r.GetProperty("gruppi"u8).GetArrayLength().Should().Be(6);
        var g = Gruppo(r, "sudoku");
        g.GetProperty("generati"u8).GetInt32().Should().Be(0);
        g.GetProperty("quota_non_validi"u8).ValueKind.Should().Be(JsonValueKind.Null);
        g.GetProperty("tipologie"u8).GetArrayLength().Should().Be(0);
    }

    [Fact]
    public void Markdown_ScriveMisuraIndicativaESoglie()
    {
        var md = CalcolatoreTempiRevisione.Calcola(Sintetici(), Adesso).InMarkdown();

        md.Should().Contain("crittografia_breve").And.Contain("misura indicativa").And.Contain("12,5");
        md.Should().NotContain("\r");
        CalcolatoreTempiRevisione.Calcola(Sintetici(), Adesso).InMarkdown().Should().Be(md);
    }

    [Fact]
    public void Soglia_UsaLaMedianaArrotondataComeMostrata()
    {
        // 1.200.002 ms = 20,0000333 minuti: mostrato 20, quindi entro la soglia di 20.
        var rapporto = Json([new CandidatoPerRapporto("anagramma", 1, "accettato", 1_200_002)]);

        var g = Gruppo(rapporto, "anagramma");
        g.GetProperty("minuti"u8).GetProperty("mediana"u8).GetDouble().Should().Be(20);
        g.GetProperty("soglia_g1"u8).GetProperty("tempo_entro_soglia"u8).GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Lettore_ConTempoNegativo_Lancia()
    {
        var json = "{\"versione_formato\":1,\"candidati\":[{\"tipologia\":\"sudoku\",\"versione_tipologia\":0,\"stato\":\"accettato\",\"tempo_attivo_ms\":-5}]}";

        var azione = () => LettoreCandidatiDaFile.Leggi(json);

        azione.Should().Throw<FormatException>().WithMessage("*negativo*");
    }

    [Fact]
    public void Markdown_HaLeColonneNonRivistiEQuotaNonValidi()
    {
        var md = CalcolatoreTempiRevisione.Calcola(Sintetici(), Adesso).InMarkdown();

        md.Should().Contain("Non rivisti").And.Contain("Quota non validi");
        // Crittografia: 2 non rivisti, quota dei non validi 0,1429.
        md.Should().Contain("| crittografia_breve | 7 | 6 | 1 | 1 | 0,1429 | 4 | 2 |");
    }

    [Fact]
    public void Lettore_ConFormatoSconosciuto_Lancia()
    {
        var azione = () => LettoreCandidatiDaFile.Leggi("{\"versione_formato\":9,\"candidati\":[]}");

        azione.Should().Throw<FormatException>();
    }
}
