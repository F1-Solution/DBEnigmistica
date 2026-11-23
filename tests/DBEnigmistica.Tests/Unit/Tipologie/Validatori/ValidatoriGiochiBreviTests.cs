using DBEnigmistica.Tipologie.Modello;
using DBEnigmistica.Tipologie.Validatori;
using FluentAssertions;
using static DBEnigmistica.Tests.Unit.Tipologie.Validatori.DatiDiProva;

namespace DBEnigmistica.Tests.Unit.Tipologie.Validatori;

/// <summary>Validatori dei giochi brevi: diagramma, anagramma, cambio di una lettera e sciarada, con parole inventate.</summary>
public class ValidatoriGiochiBreviTests
{
    private static readonly Descrittore Anagramma = Descrittore("anagramma");
    private static readonly Descrittore Cambio = Descrittore("cambio-di-lettera");
    private static readonly Descrittore Sciarada = Descrittore("sciarada");
    private static readonly Descrittore Rebus = Descrittore("rebus", 0);

    // --- diagramma-coerente-con-soluzione ---

    [Theory]
    [InlineData("4 = 4", "cena")]
    [InlineData("(4 = 4)", "cena")]
    [InlineData("2 3 = 2 3", "la mia")]
    [InlineData("6 = 1'5", "l'amata")]
    [InlineData("4 = 6", "perché")]
    public void Diagramma_ConSoluzioneCoerente_EValido(string diagramma, string soluzione) =>
        new DiagrammaCoerenteConSoluzioneValidatore().Valida(Anagramma, Breve(diagramma, null, soluzione)).EValido.Should().BeTrue();

    [Theory]
    [InlineData("4 = 5", "cena")]
    [InlineData("4 4", "cena")]
    [InlineData("4 = 2 2", "cena")]
    [InlineData("abc = 4", "cena")]
    [InlineData("4 = 4 = 4", "cena")]
    [InlineData("= 4", "cena")]
    public void Diagramma_NonCoerente_NonValido(string diagramma, string soluzione) =>
        new DiagrammaCoerenteConSoluzioneValidatore().Valida(Anagramma, Breve(diagramma, null, soluzione)).Problemi.Should().Contain(p => p.Percorso == "diagramma");

    // Il diagramma del rebus è solo l'elenco delle lunghezze delle parole della soluzione, senza segno =.
    [Theory]
    [InlineData("2 5 1 6", "ma tenda e uccisi")]
    [InlineData("(6 4)", "strana idea")]
    public void Rebus_ConLunghezzeCoerenti_EValido(string diagramma, string soluzione) =>
        new DiagrammaCoerenteConSoluzioneValidatore().Valida(Rebus, Breve(diagramma, null, soluzione)).EValido.Should().BeTrue();

    [Theory]
    [InlineData("6 5", "strana idea")]
    [InlineData("6 4 1", "strana idea")]
    [InlineData("a b", "strana idea")]
    public void Rebus_ConLunghezzeNonCoerenti_NonValido(string diagramma, string soluzione) =>
        new DiagrammaCoerenteConSoluzioneValidatore().Valida(Rebus, Breve(diagramma, null, soluzione)).Problemi.Should().Contain(p => p.Percorso == "diagramma");

    [Fact]
    public void Rebus_ConSegnoUguale_NonEUnDiagrammaDelRebus() =>
        new DiagrammaCoerenteConSoluzioneValidatore().Valida(Rebus, Breve("6 = 4", null, "strana idea")).Problemi.Should().Contain(p => p.Percorso == "diagramma");

    [Fact]
    public void Diagramma_SoluzioneNonStampata_ControllaSoloLaForma()
    {
        new DiagrammaCoerenteConSoluzioneValidatore().Valida(Anagramma, Breve("4 = 4", null, null)).EValido.Should().BeTrue();
        new DiagrammaCoerenteConSoluzioneValidatore().Valida(Anagramma, Breve("4", null, null)).EValido.Should().BeFalse();
    }

    [Fact]
    public void Diagramma_Assente_NonSiControlla() =>
        new DiagrammaCoerenteConSoluzioneValidatore().Valida(Anagramma, Breve(null, null, "cena")).EValido.Should().BeTrue();

    // --- anagramma-stesse-lettere ---

    [Theory]
    [InlineData("cane", "cena")]
    [InlineData("Mare", "RAME")]
    [InlineData("l'ago", "gola")]
    [InlineData("perché", "REPCHÉ")]
    public void Anagramma_StesseLettere_EValido(string prima, string soluzione) =>
        new AnagrammaStesseLettereValidatore().Valida(Anagramma, Breve("4 = 4", prima, soluzione)).EValido.Should().BeTrue();

    [Theory]
    [InlineData("cane", "cene")]
    [InlineData("cane", "cane")]
    [InlineData("cane", "CANE")]
    [InlineData("cane", "canee")]
    public void Anagramma_LettereDiverseOUguale_NonValido(string prima, string soluzione) =>
        new AnagrammaStesseLettereValidatore().Valida(Anagramma, Breve("4 = 4", prima, soluzione)).Problemi.Should().Contain(p => p.Percorso == "soluzione");

    [Fact]
    public void Anagramma_ConUnaSolaDelleDueParole_NonSiControlla()
    {
        new AnagrammaStesseLettereValidatore().Valida(Anagramma, Breve("4 = 4", null, "cena")).EValido.Should().BeTrue();
        new AnagrammaStesseLettereValidatore().Valida(Anagramma, Breve("4 = 4", "cane", null)).EValido.Should().BeTrue();
    }

    // --- cambio-di-una-lettera ---

    [Theory]
    [InlineData("sale", "sole")]
    [InlineData("sale", "SALI")]
    [InlineData("pòsta", "pasta")]
    public void Cambio_UnaLetteraDiversa_EValido(string prima, string soluzione) =>
        new CambioDiUnaLetteraValidatore().Valida(Cambio, Breve("4 = 4", prima, soluzione)).EValido.Should().BeTrue();

    [Theory]
    [InlineData("sale", "sale")]
    [InlineData("sale", "sola")]
    [InlineData("sale", "salse")]
    [InlineData("sale", "sal")]
    [InlineData("sale", "lase")]
    public void Cambio_ZeroDueOLunghezzaDiversa_NonValido(string prima, string soluzione) =>
        new CambioDiUnaLetteraValidatore().Valida(Cambio, Breve("4 = 4", prima, soluzione)).Problemi.Should().Contain(p => p.Percorso == "soluzione");

    [Fact]
    public void Cambio_ConUnaSolaDelleDueParole_NonSiControlla() =>
        new CambioDiUnaLetteraValidatore().Valida(Cambio, Breve("4 = 4", "sale", null)).EValido.Should().BeTrue();

    // --- sciarada-parti-coerenti ---

    [Fact]
    public void Sciarada_PartiEDiagrammaCoerenti_EValida() =>
        new SciaradaPartiCoerentiValidatore().Valida(Sciarada, Breve("3 3 = 6", null, "agosto", "ago sto")).EValido.Should().BeTrue();

    [Fact]
    public void Sciarada_PartiCheNonDannoLaSoluzione_NonValida() =>
        new SciaradaPartiCoerentiValidatore().Valida(Sciarada, Breve("3 3 = 6", null, "agosto", "ago sta")).Problemi.Should().Contain(p => p.Percorso == "parti");

    [Fact]
    public void Sciarada_DiagrammaConLunghezzeDiverseDalleParti_NonValida() =>
        new SciaradaPartiCoerentiValidatore().Valida(Sciarada, Breve("2 4 = 6", null, "agosto", "ago sto")).Problemi.Should().Contain(p => p.Percorso == "diagramma");

    [Fact]
    public void Sciarada_UnaSolaParte_NonValida() =>
        new SciaradaPartiCoerentiValidatore().Valida(Sciarada, Breve("6 = 6", null, "agosto", "agosto")).EValido.Should().BeFalse();

    [Fact]
    public void Sciarada_SenzaParti_ControllaLaSommaDelDiagramma()
    {
        new SciaradaPartiCoerentiValidatore().Valida(Sciarada, Breve("3 3 = 6", null, "agosto")).EValido.Should().BeTrue();
        new SciaradaPartiCoerentiValidatore().Valida(Sciarada, Breve("3 3 = 7", null, "agosto")).Problemi.Should().Contain(p => p.Percorso == "diagramma");
    }

    [Fact]
    public void Sciarada_PartiConAccentiEMaiuscole_SiNormalizzano() =>
        new SciaradaPartiCoerentiValidatore().Valida(Sciarada, Breve("2 3 = 5", null, "Città", "ci ttà")).EValido.Should().BeTrue();

    [Fact]
    public void Sciarada_SenzaDiagramma_ControllaLeParti() =>
        new SciaradaPartiCoerentiValidatore().Valida(Sciarada, Breve(null, null, "agosto", "ago sta")).EValido.Should().BeFalse();

    // --- catalogo ---

    [Fact]
    public void Catalogo_HaOttoValidatoriConNomiDistinti()
    {
        CatalogoValidatori.Tutti.Should().HaveCount(8);
        CatalogoValidatori.Tutti.Select(v => v.Nome).Should().OnlyHaveUniqueItems();
        CatalogoValidatori.Tutti.Should().OnlyContain(v => v.RuoliRichiesti.Count > 0);
    }
}
