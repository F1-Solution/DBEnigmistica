using System.Text.Json;
using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Tests.Unit.Creazione;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Tipologie.Descrittori;

/// <summary>I descrittori v1 delle quattro Tipologie della 1.2 sono conformi al meta-schema e compatibili con la creazione (R1, R4).</summary>
public class DescrittoriV1Tests
{
    private static readonly ValidatoreSchemaJson Validatore = new();

    private static string MetaSchema() => FileDelRepository.Leggi("tipologie", "_meta", "descrittore.schema.json");

    public static TheoryData<string, string, string> Quattro => new()
    {
        { "cruciverba-classico", "cruciverba", "griglia" },
        { "anagramma", "gioco-breve", "esposto_diagramma" },
        { "sciarada", "gioco-breve", "esposto_diagramma" },
        { "cambio-di-lettera", "gioco-breve", "esposto_diagramma" },
    };

    public static TheoryData<string> Cartelle => new() { "cruciverba-classico", "anagramma", "sciarada", "cambio-di-lettera" };

    public static TheoryData<string> TuttiIFile()
    {
        var dati = new TheoryData<string>();
        var radice = FileDelRepository.Radice();
        var meta = Path.DirectorySeparatorChar + "_meta" + Path.DirectorySeparatorChar;
        foreach (var f in Directory.GetFiles(FileDelRepository.Percorso("tipologie"), "v*.json", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
            if (!f.Contains(meta, StringComparison.Ordinal))
                dati.Add(Path.GetRelativePath(radice, f));
        return dati;
    }

    [Theory]
    [MemberData(nameof(Cartelle))]
    public void Descrittore_V1_ConformeAlMetaSchema(string id)
    {
        var json = FileDelRepository.Leggi("tipologie", id, "v1.json");

        Validatore.Valida(json, MetaSchema()).Should().BeEmpty();
    }

    [Fact]
    public void MetaSchema_RifiutaUnDescrittoreNonValido()
    {
        var json = FileDelRepository.Leggi("tipologie", "_meta", "esempi", "descrittore", "non-valido-tipo.json");

        Validatore.Valida(json, MetaSchema()).Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Quattro))]
    public void Descrittore_V1_HaIdentitaFamigliaERisaAttese(string id, string famiglia, string resa)
    {
        var d = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", id, "v1.json"));

        d.Id.Should().Be(id);
        d.Versione.Should().Be(1);
        d.Famiglia.Should().Be(famiglia);
        d.Resa.Should().Be(resa);
    }

    [Theory]
    [MemberData(nameof(Cartelle))]
    public void Descrittore_V1_HaRegoleInBozzaVersione1(string id)
    {
        var d = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", id, "v1.json"));

        d.Regole.Approvata.Should().BeFalse();
        d.Regole.Versione.Should().Be(1);
        d.Regole.Testo.Length.Should().BeGreaterThan(200);
    }

    [Theory]
    [MemberData(nameof(Cartelle))]
    public void Descrittore_V1_HaElenchiPresentiENessunaImmagine(string id)
    {
        var json = FileDelRepository.Leggi("tipologie", id, "v1.json");
        using var doc = JsonDocument.Parse(json);
        var d = DescrittoreTipologia.Leggi(json);

        doc.RootElement.TryGetProperty("elenchi"u8, out _).Should().BeTrue();
        d.Campi.Concat(d.Elenchi.SelectMany(e => e.Campi)).Should().NotContain(c => c.Tipo == DescrittoreTipologia.TipoImmagine);
    }

    [Theory]
    [MemberData(nameof(TuttiIFile))]
    public void OgniDescrittore_SiLegge_ConIlLettoreDellaCreazione(string percorso)
    {
        var completo = Path.Combine(FileDelRepository.Radice(), percorso);

        var d = DescrittoreTipologia.Leggi(File.ReadAllText(completo));

        d.Id.Should().Be(new DirectoryInfo(Path.GetDirectoryName(completo)!).Name);
        Path.GetFileNameWithoutExtension(percorso).Should().Be("v" + d.Versione);
    }

    [Fact]
    public void TuttiIFile_ComprendeiCinqueV0EiQuattroV1()
    {
        TuttiIFile().Should().HaveCountGreaterThanOrEqualTo(9);
    }

    [Theory]
    [InlineData("cruciverba-classico")]
    [InlineData("anagramma")]
    public void DerivatoreSchemaCreazione_Generico_FunzionaSuiDescrittoriV1(string id)
    {
        var d = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", id, "v1.json"));

        var schema = DerivatoreSchemaCreazione.Generico(d);

        schema.Length.Should().BeInRange(1, DerivatoreSchemaCreazione.LunghezzaMassimaSchema);
        using var _ = JsonDocument.Parse(schema);
    }

    [Fact]
    public void Cruciverba_V1_HaElencoDefinizioniConRuoliDellaResaGriglia()
    {
        var d = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "cruciverba-classico", "v1.json"));

        d.Campi.Select(c => c.Ruolo).Should().Contain(["righe", "colonne", "griglia"]);
        d.Elenchi.Should().ContainSingle(e => e.Nome == "definizioni")
            .Which.Campi.Select(c => c.Ruolo).Should().Contain(["numero", "direzione", "definizione"]);
    }
}
