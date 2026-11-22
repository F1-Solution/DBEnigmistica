using DBEnigmistica.Application.Valutazione;
using FluentAssertions;
using static DBEnigmistica.Tests.Unit.Valutazione.ServiziDiProva;

namespace DBEnigmistica.Tests.Unit.Valutazione;

public class LineaDiBaseTests
{
    private static async Task<RapportoRegressione> Misura()
    {
        var risultato = await Servizio().EseguiAsync(Parametri(SetVersionato), Riproduci(SetVersionato));
        return risultato.Rapporto;
    }

    [Fact]
    public async Task Confronta_ConLaMisuraStessa_NessunaRegressione()
    {
        var rapporto = await Misura();

        LineaDiBase.DaRapporto(rapporto).Confronta(rapporto).Should().BeEmpty();
    }

    [Theory]
    [InlineData("richiamo")]
    [InlineData("precisione")]
    [InlineData("tipologia_corretta")]
    [InlineData("caselle_corrette")]
    [InlineData("campi_esatti")]
    public async Task Confronta_UnaMetricaDiMeritoCheScende_ERegressione(string metrica)
    {
        var rapporto = await Misura();
        var migliore = LineaDiBase.DaRapporto(rapporto).Con("totale", metrica, 1.0);
        var peggiorata = rapporto with { Gruppi = rapporto.Gruppi.ToDictionary(g => g.Key, g => g.Key == "totale" ? Abbassa(g.Value, metrica) : g.Value) };

        var regressioni = migliore.Confronta(peggiorata);

        regressioni.Should().ContainSingle().Which.Should().Contain(metrica).And.Contain("totale");
    }

    private static MetricheGruppo Abbassa(MetricheGruppo g, string metrica) => metrica switch
    {
        "richiamo" => g with { Richiamo = 0.5 },
        "precisione" => g with { Precisione = 0.5 },
        "tipologia_corretta" => g with { TipologiaCorretta = 0.5 },
        "caselle_corrette" => g with { CaselleCorrette = 0.5 },
        _ => g with { CampiEsatti = 0.5 }
    };

    [Fact]
    public async Task Confronta_IlCerCheSale_ERegressione_QuelloCheScendeNo()
    {
        var rapporto = await Misura();
        var cer = rapporto.Gruppi["totale"].Cer!.Value;

        LineaDiBase.DaRapporto(rapporto).Con("totale", "cer", cer - 0.0001).Confronta(rapporto).Should().ContainSingle().Which.Should().Contain("cer");
        LineaDiBase.DaRapporto(rapporto).Con("totale", "cer", cer + 0.0001).Confronta(rapporto).Should().BeEmpty();
    }

    [Fact]
    public async Task Confronta_UnaMetricaCheMigliora_NonERegressione()
    {
        var rapporto = await Misura();

        LineaDiBase.DaRapporto(rapporto).Con("totale", "precisione", 0.1).Confronta(rapporto).Should().BeEmpty();
    }

    [Fact]
    public async Task Confronta_ValoreNellaLineaDiBaseEnulloNellaMisura_ERegressione()
    {
        var rapporto = await Misura();
        var senzaCer = rapporto with { Gruppi = rapporto.Gruppi.ToDictionary(g => g.Key, g => g.Value with { Cer = null }) };

        LineaDiBase.DaRapporto(rapporto).Confronta(senzaCer).Should().Contain(r => r.Contains("cer", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Confronta_ValoreNulloNellaLineaDiBase_NonSiConfronta()
    {
        var rapporto = await Misura();

        LineaDiBase.DaRapporto(rapporto).Con("totale", "richiamo", null).Confronta(rapporto with
        {
            Gruppi = rapporto.Gruppi.ToDictionary(g => g.Key, g => g.Value with { Richiamo = 0.1 })
        }).Should().NotContain(r => r.Contains("richiamo (totale)", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Confronta_ConCondizioniDiverse_ErroreCheChiedeUnaLineaDiBaseNuova()
    {
        var rapporto = await Misura();
        var altra = LineaDiBase.DaRapporto(rapporto with { Condizioni = rapporto.Condizioni with { VersionePrompt = "estrazione-v1-000000000000" } });

        var azione = () => altra.Confronta(rapporto);

        azione.Should().Throw<ValutazioneException>().WithMessage("*Linea di base*nuova*");
    }

    [Fact]
    public async Task Confronta_ConSetDiversa_Errore()
    {
        var rapporto = await Misura();
        var altra = LineaDiBase.DaRapporto(rapporto with { Set = "altro" });

        var azione = () => altra.Confronta(rapporto);

        azione.Should().Throw<ValutazioneException>().WithMessage("*altro*");
    }

    [Fact]
    public async Task InJson_PoiInterpreta_RestituisceLaStessaLineaDiBase()
    {
        var rapporto = await Misura();
        var linea = LineaDiBase.DaRapporto(rapporto);

        var riletta = LineaDiBase.Interpreta(linea.InJson());

        riletta.InJson().Should().Be(linea.InJson());
        riletta.Confronta(rapporto).Should().BeEmpty();
    }

    [Fact]
    public void Interpreta_JsonNonValido_Errore()
    {
        var azione = () => LineaDiBase.Interpreta("{}");

        azione.Should().Throw<ValutazioneException>();
    }

    [Fact]
    public async Task Servizio_ConLineaDiBasePeggioreDellaMisura_ElencaLeRegressioni()
    {
        var radice = Path.Combine(Path.GetTempPath(), "dbe-linea-" + Guid.NewGuid().ToString("N"));
        try
        {
            CopiaCartella(SetVersionato, radice);
            var rapporto = await Misura();
            await File.WriteAllTextAsync(Path.Combine(radice, "linea-di-base.json"), LineaDiBase.DaRapporto(rapporto).Con("totale", "richiamo", 1.0).Con("totale", "precisione", 1.0).InJson());

            var risultato = await Servizio().EseguiAsync(Parametri(radice), Riproduci(radice));

            risultato.ConfrontoDisponibile.Should().BeTrue();
            risultato.Regressioni.Should().ContainSingle().Which.Should().Contain("precisione");
            risultato.Markdown.Should().Contain("Regressione").And.Contain("precisione");
        }
        finally
        {
            Directory.Delete(radice, recursive: true);
        }
    }
}
