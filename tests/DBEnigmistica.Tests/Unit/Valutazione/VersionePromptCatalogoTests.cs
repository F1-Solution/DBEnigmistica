using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Valutazione;
using DBEnigmistica.Tests.Unit.Creazione;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Catalogo;
using DBEnigmistica.Tipologie.Derivazione;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Valutazione;

public class VersionePromptCatalogoTests
{
    private static ModelloPromptEstrazione DaFile(IReadOnlyList<DBEnigmistica.Tipologie.Modello.Descrittore> descrittori) =>
        ModelloPromptEstrazione.Carica(
            FileDelRepository.Leggi("tipologie", "_meta", "prompt-estrazione-v1.md"),
            FormattatoreCatalogo.Formatta(descrittori), DerivatoreSchemaPagina.Deriva(descrittori));

    [Fact]
    public void Versione_HaLaFormaDellaSezione53()
    {
        new VersionePromptCatalogo().VersionePrompt.Should().MatchRegex("^estrazione-v1-[0-9a-f]{12}$");
    }

    [Fact]
    public void Versione_ECalcolataDalCatalogoEDalloSchemaDerivatoDeiFileDelRepository()
    {
        var descrittori = new CaricatoreDescrittori().CaricaCartella(FileDelRepository.Percorso("tipologie")).PerEstrazione();

        var atteso = DaFile(descrittori);
        var versione = new VersionePromptCatalogo();

        versione.VersionePrompt.Should().Be(atteso.VersionePrompt);
        versione.JsonSchema.Should().Be(atteso.JsonSchema);
    }

    [Fact]
    public void Catalogo_HaLeQuattroTipologieDiEstrazioneESenzaPrototipi()
    {
        var versione = new VersionePromptCatalogo();

        versione.Descrittori.Select(d => d.Id).Should().Equal("anagramma", "cambio-di-lettera", "cruciverba-classico", "sciarada");
        versione.Descrittori.Should().OnlyContain(d => d.Versione > 0);
    }

    [Fact]
    public void Versione_CambiaSeCambiaUnaRegolaDiUnDescrittore()
    {
        var originali = new VersionePromptCatalogo().Descrittori;
        var modificati = originali
            .Select(d => d.Id == "anagramma" ? d with { Regole = d.Regole with { Testo = d.Regole.Testo + " Una frase in più." } } : d)
            .ToList();

        DaFile(modificati).VersionePrompt.Should().NotBe(new VersionePromptCatalogo().VersionePrompt);
    }

    [Fact]
    public void Versione_ECostanteTraDueCaricamenti()
    {
        new VersionePromptCatalogo().VersionePrompt.Should().Be(new VersionePromptCatalogo().VersionePrompt);
    }
}
