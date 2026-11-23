using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Tests.Integration.Prototipo;
using DBEnigmistica.Infrastructure.Prototipo;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Creazione;

/// <summary>Elenco delle Tipologie del prototipo con i campi del brief ammessi per famiglia (preparazione 1b.2, R5).</summary>
public sealed class TipologieDelPrototipoTests
{
    private static async Task<IReadOnlyList<TipologiaPrototipo>> ElencoAsync() =>
        await new TipologieDelPrototipo(new CatalogoTipologieFileSystem(FileDelRepository.Percorso("tipologie"))).EseguiAsync();

    private static TipologiaPrototipo Tipo(IReadOnlyList<TipologiaPrototipo> e, string id) => e.Single(t => t.Id == id);

    private static CampoBrief Campo(TipologiaPrototipo t, string nome) => t.CampiBrief.Single(c => c.Nome == nome);

    [Fact]
    public async Task Elenca_SoloLeSetteTipologieDelPrototipoNellUltimaVersione()
    {
        var e = await ElencoAsync();

        e.Select(t => t.Id).Should().Equal("anagramma", "crittografia-breve", "cruciverba-classico", "futoshiki", "nonogramma", "rebus", "sudoku");
        Tipo(e, "anagramma").Versione.Should().Be(1);
        Tipo(e, "crittografia-breve").Versione.Should().Be(0);
        e.Should().OnlyContain(t => !t.RegoleApprovate && t.StatoRegole == "bozza");
    }

    [Fact]
    public async Task Modo_ELogicoSiGeneraConAlgoritmoLeAltreConAi()
    {
        var e = await ElencoAsync();

        e.Where(t => t.Modo == "algoritmo").Select(t => t.Id).Should().Equal("futoshiki", "nonogramma", "sudoku");
        e.Where(t => t.Modo == "ai").Select(t => t.Id).Should().Equal("anagramma", "crittografia-breve", "cruciverba-classico", "rebus");
    }

    [Fact]
    public async Task Campi_Crittografia_HaSottotipoObbligatorioENienteDimensioni()
    {
        var t = Tipo(await ElencoAsync(), "crittografia-breve");

        Campo(t, "sottotipo").Should().Match<CampoBrief>(c => c.Tipo == "scelta" && c.Obbligatorio && c.Valori.SequenceEqual(new[] { "sinonimica", "perifrastica" }));
        t.CampiBrief.Select(c => c.Nome).Should().NotContain("dimensioni");
        Campo(t, "tema").Obbligatorio.Should().BeFalse();
        Campo(t, "difficolta").Should().Match<CampoBrief>(c => c.Minimo == 1 && c.Massimo == 5 && c.Obbligatorio);
        Campo(t, "vincoli").Massimo.Should().Be(8);
    }

    [Fact]
    public async Task Campi_Anagramma_HaTemaObbligatorio()
    {
        var t = Tipo(await ElencoAsync(), "anagramma");

        Campo(t, "tema").Obbligatorio.Should().BeTrue();
        t.CampiBrief.Select(c => c.Nome).Should().NotContain(["dimensioni", "sottotipo"]);
    }

    [Fact]
    public async Task Campi_Cruciverba_HaDimensioniQuadrateDaNoveATredici()
    {
        var t = Tipo(await ElencoAsync(), "cruciverba-classico");

        Campo(t, "dimensioni").Should().Match<CampoBrief>(c => c.Tipo == "dimensioni-quadrate" && c.Obbligatorio && c.Valori.SequenceEqual(new[] { "9", "10", "11", "12", "13" }));
    }

    [Fact]
    public async Task Campi_Logici_NonHannoTemaNeVincoliEHannoLeDimensioniDellaTipologia()
    {
        var e = await ElencoAsync();

        foreach (var id in new[] { "sudoku", "futoshiki", "nonogramma" })
            Tipo(e, id).CampiBrief.Select(c => c.Nome).Should().NotContain(["tema", "vincoli", "sottotipo"]);
        Campo(Tipo(e, "sudoku"), "dimensioni").Valori.Should().Equal("9");
        Campo(Tipo(e, "futoshiki"), "dimensioni").Valori.Should().Equal("5", "6", "7");
        Campo(Tipo(e, "nonogramma"), "dimensioni").Should().Match<CampoBrief>(c => c.Tipo == "dimensioni" && c.Minimo == 10 && c.Massimo == 15);
    }

    [Fact]
    public async Task Campi_Rebus_NonHaDimensioni()
    {
        Tipo(await ElencoAsync(), "rebus").CampiBrief.Select(c => c.Nome).Should().Equal("tema", "vincoli", "difficolta");
    }

    private static async Task<IReadOnlyList<TipologiaPrototipo>> ConFornitoreAsync(IFornitoreAiCreazione? fornitore, bool approvate = false)
    {
        var catalogo = new CatalogoTipologieFileSystem(FileDelRepository.Percorso("tipologie"));
        if (!approvate)
            return await new TipologieDelPrototipo(catalogo, fornitore).EseguiAsync();

        var tutte = await catalogo.ElencaAsync();
        var approvati = tutte.Select(d => d with { Regole = d.Regole with { Approvata = true, ApprovateIl = "2026-12-01" } }).ToArray();
        return await new TipologieDelPrototipo(new CatalogoInMemoria(approvati), fornitore).EseguiAsync();
    }

    [Fact]
    public async Task GenerazioneConsentita_DalVivoConRegoleInBozza_SoloPerGliAlgoritmi()
    {
        var e = await ConFornitoreAsync(new FornitoreCreazioneFinto());

        e.Where(t => t.GenerazioneConsentita).Select(t => t.Id).Should().Equal("futoshiki", "nonogramma", "sudoku");
    }

    [Fact]
    public async Task GenerazioneConsentita_InRiproduzioneConRegoleInBozza_PerTutte()
    {
        var e = await ConFornitoreAsync(new FornitoreAiCreazioneInRiproduzione(new FornitoreCreazioneFinto()));

        e.Should().OnlyContain(t => t.GenerazioneConsentita);
    }

    [Fact]
    public async Task GenerazioneConsentita_ConRegoleApprovateDalVivo_PerTutte()
    {
        var e = await ConFornitoreAsync(new FornitoreCreazioneFinto(), approvate: true);

        e.Should().OnlyContain(t => t.GenerazioneConsentita);
    }

    [Fact]
    public async Task GenerazioneConsentita_ConInterruttoreSpento_SoloPerGliAlgoritmiAncheConRegoleApprovate()
    {
        var e = await ConFornitoreAsync(new FornitoreAiCreazioneDisattivato("spento"), approvate: true);

        e.Where(t => t.GenerazioneConsentita).Select(t => t.Id).Should().Equal("futoshiki", "nonogramma", "sudoku");
    }
}
