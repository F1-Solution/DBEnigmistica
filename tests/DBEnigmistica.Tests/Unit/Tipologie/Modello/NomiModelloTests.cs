using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Tipologie.Modello;

/// <summary>I nomi dei valori chiusi del modello coincidono con le enumerazioni del meta-schema.</summary>
public class NomiModelloTests
{
    private static JsonElement Definizioni()
    {
        var percorso = Path.Combine(Unit.Creazione.FileDelRepository.Radice(), "tipologie", "_meta", "descrittore.schema.json");
        using var documento = JsonDocument.Parse(File.ReadAllText(percorso));
        return documento.RootElement.GetProperty("$defs"u8).Clone();
    }

    private static string[] Enumerazione(string definizione) =>
        [.. Definizioni().GetProperty(definizione).GetProperty("enum"u8).EnumerateArray().Select(e => e.GetString()!)];

    private static string[] Nomi<T>() where T : struct, Enum => [.. Enum.GetValues<T>().Select(NomiModello.Nome)];

    [Fact]
    public void Famiglie_coincidono_con_il_meta_schema()
    {
        using var documento = JsonDocument.Parse(File.ReadAllText(Path.Combine(Unit.Creazione.FileDelRepository.Radice(), "tipologie", "_meta", "descrittore.schema.json")));
        var attese = documento.RootElement.GetProperty("properties"u8).GetProperty("famiglia"u8).GetProperty("enum"u8)
            .EnumerateArray().Select(e => e.GetString()!).ToList();

        Nomi<FamigliaTipologia>().Should().BeEquivalentTo(attese);
    }

    [Fact]
    public void Tipi_coincidono_con_il_meta_schema() => Nomi<TipoCampo>().Should().BeEquivalentTo(Enumerazione("tipo"));

    [Fact]
    public void Ruoli_coincidono_con_il_meta_schema() => Nomi<RuoloCampo>().Should().BeEquivalentTo(Enumerazione("ruolo"));

    [Fact]
    public void Validatori_coincidono_con_il_meta_schema() => Nomi<NomeValidatore>().Should().BeEquivalentTo(Enumerazione("validatore"));

    [Fact]
    public void Modelli_di_resa_coincidono_con_il_meta_schema()
    {
        var attesi = Definizioni().GetProperty("resa"u8).GetProperty("properties"u8).GetProperty("modello"u8).GetProperty("enum"u8)
            .EnumerateArray().Select(e => e.GetString()!);

        Nomi<ModelloResa>().Should().BeEquivalentTo(attesi);
    }

    [Fact]
    public void Ogni_nome_si_rilegge_nello_stesso_valore()
    {
        foreach (var v in Enum.GetValues<TipoCampo>())
            NomiModello.TryLeggi<TipoCampo>(NomiModello.Nome(v), out var letto).Should().BeTrue();
        NomiModello.TryLeggi<RuoloCampo>("definizione", out var ruolo).Should().BeTrue();
        ruolo.Should().Be(RuoloCampo.Definizione);
        NomiModello.TryLeggi<NomeValidatore>("anagramma-stesse-lettere", out var validatore).Should().BeTrue();
        validatore.Should().Be(NomeValidatore.AnagrammaStesseLettere);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Testo")]
    [InlineData("inesistente")]
    public void Nome_sconosciuto_non_si_legge(string nome) => NomiModello.TryLeggi<TipoCampo>(nome, out _).Should().BeFalse();

    [Fact]
    public void Esito_valido_non_ha_problemi_e_quello_non_valido_li_conserva()
    {
        EsitoValidazione.Valido.EValido.Should().BeTrue();
        var esito = EsitoValidazione.NonValido(new ProblemaValidazione("griglia", "dimensioni incoerenti"));

        esito.EValido.Should().BeFalse();
        esito.Problemi.Should().ContainSingle().Which.Percorso.Should().Be("griglia");
    }

    [Fact]
    public void Esito_non_valido_senza_problemi_e_rifiutato()
    {
        var azione = () => EsitoValidazione.NonValido([]);

        azione.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Descrittore_cerca_campi_ed_elenchi_per_ruolo()
    {
        var griglia = new CampoDescrittore("g", TipoCampo.Griglia, Ruolo: RuoloCampo.Griglia);
        var elenco = new ElencoDescrittore("definizioni", [new CampoDescrittore("n", TipoCampo.Intero, Ruolo: RuoloCampo.Numero)]);
        var descrittore = new Descrittore(
            "x", 1, FamigliaTipologia.Cruciverba, "X", "d",
            new RegoleTipologia("t", 1, StatoRegole.Bozza), [griglia], [elenco], new ResaDescrittore(ModelloResa.Griglia), []);

        descrittore.CampoConRuolo(RuoloCampo.Griglia).Should().Be(griglia);
        descrittore.CampoConRuolo(RuoloCampo.Righe).Should().BeNull();
        descrittore.CampoDiElencoConRuolo(RuoloCampo.Numero)!.Nome.Should().Be("n");
    }
}
