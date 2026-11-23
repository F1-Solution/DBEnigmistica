using DBEnigmistica.Tipologie.Modello;
using DBEnigmistica.Tipologie.Validatori;
using FluentAssertions;
using static DBEnigmistica.Tests.Unit.Tipologie.Validatori.DatiDiProva;

namespace DBEnigmistica.Tests.Unit.Tipologie.Validatori;

/// <summary>Validatori del cruciverba: dimensioni, numerazione, lunghezze e incroci, con dati inventati.</summary>
public class ValidatoriGrigliaTests
{
    private static readonly Descrittore Cruciverba3 = Descrittore("cruciverba-classico");

    private static EsitoValidazione Valida(IValidatoreStrutturale validatore, System.Text.Json.JsonElement dati) =>
        validatore.Valida(Cruciverba3, dati);

    // --- griglia-coerente-con-dimensioni ---

    [Fact]
    public void Dimensioni_GrigliaCoerente_EValida() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Cruciverba()).EValido.Should().BeTrue();

    [Fact]
    public void Dimensioni_RigheDeiDatiDiverseDallaGriglia_NonValido() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Cruciverba(righe: 4)).Problemi.Should().Contain(p => p.Percorso == "righe");

    [Fact]
    public void Dimensioni_ColonneDellaGrigliaDiverseDaiDati_NonValido() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Cruciverba(grigliaColonne: 4)).EValido.Should().BeFalse();

    [Fact]
    public void Dimensioni_RigaPiuCorta_NonValido() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Cruciverba(celle: """["CA#","AM","#AR"]""")).Problemi.Should().Contain(p => p.Percorso == "griglia.celle[1]");

    [Fact]
    public void Dimensioni_TroppeRighe_NonValido() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Cruciverba(celle: """["CA#","AMO","#AR","..."]""")).EValido.Should().BeFalse();

    [Fact]
    public void Dimensioni_SimboloNonAmmesso_NonValido() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Cruciverba(celle: """["CA#","AMO","#A!"]""")).Problemi.Should().Contain(p => p.Percorso == "griglia.celle[2]");

    [Fact]
    public void Dimensioni_NumeroSuCasellaNera_NonValido() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Cruciverba(numeri: """[{"riga":1,"colonna":3,"numero":1}]""")).EValido.Should().BeFalse();

    [Fact]
    public void Dimensioni_NumeroFuoriDallaGriglia_NonValido() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Cruciverba(numeri: """[{"riga":4,"colonna":1,"numero":1}]""")).EValido.Should().BeFalse();

    [Fact]
    public void Dimensioni_GrigliaMancante_NonValido() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Json("""{"righe":3,"colonne":3,"griglia":null}""")).EValido.Should().BeFalse();

    [Fact]
    public void Dimensioni_CifreSoloNellaResaLogica()
    {
        var logico = Descrittore("sudoku", 0);
        var dati = Json("""{"righe":2,"colonne":2,"griglia":{"righe":2,"colonne":2,"celle":["1.",".2"],"numeri":[]}}""");

        new GrigliaCoerenteConDimensioniValidatore().Valida(logico, dati).EValido.Should().BeTrue();
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Cruciverba(celle: """["C1#","AMO","#AR"]""")).EValido.Should().BeFalse();
    }

    [Fact]
    public void Validatori_DatiNonOggetto_NonValido() =>
        Valida(new GrigliaCoerenteConDimensioniValidatore(), Json("[1]")).EValido.Should().BeFalse();

    [Fact]
    public void Validatori_DescrittoreSenzaRuoli_NonValido()
    {
        var anagramma = Descrittore("anagramma");

        new GrigliaCoerenteConDimensioniValidatore().Valida(anagramma, Json("{}")).Problemi.Should().Contain(p => p.Percorso == "descrittore");
    }

    // --- numerazione-coerente-con-griglia ---

    [Fact]
    public void Numerazione_Corretta_EValida() =>
        Valida(new NumerazioneCoerenteConGrigliaValidatore(), Cruciverba()).EValido.Should().BeTrue();

    [Fact]
    public void Numerazione_NumeroMancanteNellaGriglia_NonValido() =>
        Valida(new NumerazioneCoerenteConGrigliaValidatore(), Cruciverba(numeri: """[{"riga":1,"colonna":1,"numero":1},{"riga":1,"colonna":2,"numero":2},{"riga":2,"colonna":1,"numero":3},{"riga":2,"colonna":3,"numero":4}]"""))
            .EValido.Should().BeFalse();

    [Fact]
    public void Numerazione_NumeriScambiati_NonValido() =>
        Valida(new NumerazioneCoerenteConGrigliaValidatore(), Cruciverba(numeri: """[{"riga":1,"colonna":1,"numero":1},{"riga":1,"colonna":2,"numero":3},{"riga":2,"colonna":1,"numero":2},{"riga":2,"colonna":3,"numero":4},{"riga":3,"colonna":2,"numero":5}]"""))
            .EValido.Should().BeFalse();

    [Fact]
    public void Numerazione_NumeroInCasellaCheNonIniziaUnaParola_NonValido() =>
        Valida(new NumerazioneCoerenteConGrigliaValidatore(), Cruciverba(numeri: Numeri.Replace("]", ",{\"riga\":2,\"colonna\":2,\"numero\":6}]", StringComparison.Ordinal)))
            .EValido.Should().BeFalse();

    [Fact]
    public void Numerazione_DefinizioneDiUnaParolaInesistente_NonValido() =>
        Valida(new NumerazioneCoerenteConGrigliaValidatore(), Cruciverba(definizioni: Definizioni.Replace("\"numero\":5", "\"numero\":9", StringComparison.Ordinal)))
            .Problemi.Should().Contain(p => p.Percorso == "definizioni[5]");

    [Fact]
    public void Numerazione_DirezioneNonPossibile_NonValido() =>
        Valida(new NumerazioneCoerenteConGrigliaValidatore(), Cruciverba(definizioni: Definizioni.Replace("\"numero\":4,\"direzione\":\"verticale\"", "\"numero\":4,\"direzione\":\"orizzontale\"", StringComparison.Ordinal)))
            .EValido.Should().BeFalse();

    [Fact]
    public void Numerazione_DefinizioneMancante_NonValido() =>
        Valida(new NumerazioneCoerenteConGrigliaValidatore(), Cruciverba(definizioni: """[{"numero":1,"direzione":"orizzontale","testo":"a","soluzione":null}]"""))
            .EValido.Should().BeFalse();

    [Fact]
    public void Numerazione_DefinizioneDoppia_NonValido() =>
        Valida(new NumerazioneCoerenteConGrigliaValidatore(), Cruciverba(definizioni: Definizioni.Replace("]", ",{\"numero\":5,\"direzione\":\"orizzontale\",\"testo\":\"bis\",\"soluzione\":null}]", StringComparison.Ordinal)))
            .Problemi.Should().Contain(p => p.Percorso == "definizioni[6]");

    // --- lunghezze-coerenti ---

    [Fact]
    public void Lunghezze_Corrette_SonoValide() =>
        Valida(new LunghezzeCoerentiValidatore(), Cruciverba()).EValido.Should().BeTrue();

    [Fact]
    public void Lunghezze_SoluzioneTroppoLunga_NonValido() =>
        Valida(new LunghezzeCoerentiValidatore(), Cruciverba(definizioni: Definizioni.Replace("\"soluzione\":\"CA\"}", "\"soluzione\":\"CAS\"}", StringComparison.Ordinal)))
            .Problemi.Should().Contain(p => p.Percorso == "definizioni[0].soluzione");

    [Fact]
    public void Lunghezze_AccentiEApostrofiNonContanoComeLettere() =>
        Valida(new LunghezzeCoerentiValidatore(), Cruciverba(definizioni: Definizioni.Replace("\"soluzione\":\"CA\"}", "\"soluzione\":\"cà\"}", StringComparison.Ordinal).Replace("\"soluzione\":\"AMO\"", "\"soluzione\":\"a'mo\"", StringComparison.Ordinal)))
            .EValido.Should().BeTrue();

    [Fact]
    public void Lunghezze_SoluzioneAssente_NonSiControlla() =>
        Valida(new LunghezzeCoerentiValidatore(), Cruciverba(definizioni: Definizioni.Replace("\"soluzione\":\"AMA\"", "\"soluzione\":null", StringComparison.Ordinal)))
            .EValido.Should().BeTrue();

    [Fact]
    public void Lunghezze_DiagrammaNellaDefinizioneDiversoDallaParola_NonValido() =>
        Valida(new LunghezzeCoerentiValidatore(), Cruciverba(definizioni: Definizioni.Replace("Ama (3)", "Ama (4)", StringComparison.Ordinal)))
            .Problemi.Should().Contain(p => p.Percorso == "definizioni[2].testo");

    [Fact]
    public void Lunghezze_DiagrammaConPiuNumeri_SiSomma() =>
        Valida(new LunghezzeCoerentiValidatore(), Cruciverba(definizioni: Definizioni.Replace("Ama (3)", "Ama (1,2)", StringComparison.Ordinal)))
            .EValido.Should().BeTrue();

    [Fact]
    public void Lunghezze_NumeroFinaleChePariDiUnAnno_NonEUnDiagramma() =>
        Valida(new LunghezzeCoerentiValidatore(), Cruciverba(definizioni: Definizioni.Replace("Ama (3)", "Anno (1999)", StringComparison.Ordinal)))
            .EValido.Should().BeTrue();

    // --- incroci-coerenti ---

    [Fact]
    public void Incroci_Coerenti_SonoValidi() =>
        Valida(new IncrociCoerentiValidatore(), Cruciverba()).EValido.Should().BeTrue();

    [Fact]
    public void Incroci_DueSoluzioniDiscordiNellIncrocio_NonValido() =>
        Valida(new IncrociCoerentiValidatore(), Cruciverba(definizioni: Definizioni.Replace("\"soluzione\":\"AR\"", "\"soluzione\":\"AL\"", StringComparison.Ordinal)))
            .Problemi.Should().Contain(p => p.Percorso == "definizioni[5].soluzione");

    [Fact]
    public void Incroci_SoluzioneDiversaDallaLetteraStampata_NonValido() =>
        Valida(new IncrociCoerentiValidatore(), Cruciverba(
            celle: """["CA#","AMO","#AR"]""",
            definizioni: """[{"numero":3,"direzione":"orizzontale","testo":"x","soluzione":"AMI"}]"""))
            .Problemi.Should().Contain(p => p.Percorso == "definizioni[0].soluzione");

    [Fact]
    public void Incroci_MaiuscoleEAccentiSiNormalizzano() =>
        Valida(new IncrociCoerentiValidatore(), Cruciverba(definizioni: Definizioni.Replace("\"soluzione\":\"AMO\"", "\"soluzione\":\"àmo\"", StringComparison.Ordinal)))
            .EValido.Should().BeTrue();

    [Fact]
    public void Incroci_SoluzioneDiLunghezzaSbagliata_NonSiControllaQui() =>
        Valida(new IncrociCoerentiValidatore(), Cruciverba(definizioni: Definizioni.Replace("\"soluzione\":\"AMO\"", "\"soluzione\":\"AMOXX\"", StringComparison.Ordinal)))
            .EValido.Should().BeTrue();

    [Fact]
    public void Incroci_GrigliaVuotaDiLettere_SiControllanoSoloLeSoluzioni() =>
        Valida(new IncrociCoerentiValidatore(), Cruciverba(celle: """["..#","...","#.."]""")).EValido.Should().BeTrue();
}
