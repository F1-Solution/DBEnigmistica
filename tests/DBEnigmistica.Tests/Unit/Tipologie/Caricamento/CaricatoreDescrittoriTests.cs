using System.Text.Json.Nodes;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Modello;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Tipologie.Caricamento;

public class CaricatoreDescrittoriTests
{
    private static readonly CaricatoreDescrittori Caricatore = new();

    private static string Cruciverba() => PercorsiTipologie.Esempio("descrittore", "valido-esempio-cruciverba.v1.json");

    private static string Modificato(Action<JsonObject> modifica)
    {
        var nodo = JsonNode.Parse(Cruciverba())!.AsObject();
        modifica(nodo);
        return nodo.ToJsonString();
    }

    private static string[] Errori(string json, CaricatoreDescrittori? caricatore = null)
    {
        var azione = () => (caricatore ?? Caricatore).CaricaTesto(json);
        return azione.Should().Throw<DescrittoreNonValidoException>().Which.Errori.ToArray();
    }

    [Fact]
    public void Legge_i_due_esempi_validi_nei_tipi_del_modello()
    {
        var cruciverba = PercorsiTipologie.DescrittoreEsempio("valido-esempio-cruciverba.v1.json");
        var anagramma = PercorsiTipologie.DescrittoreEsempio("valido-esempio-anagramma.v1.json");

        cruciverba.Id.Should().Be("esempio-cruciverba");
        cruciverba.Famiglia.Should().Be(FamigliaTipologia.Cruciverba);
        cruciverba.Resa.Modello.Should().Be(ModelloResa.Griglia);
        cruciverba.Regole.Stato.Should().Be(StatoRegole.Bozza);
        cruciverba.CampoConRuolo(RuoloCampo.Griglia)!.Nome.Should().Be("griglia");
        cruciverba.Elenchi.Should().ContainSingle().Which.Campi.Should().HaveCount(4);
        cruciverba.Validatori.Should().StartWith(NomeValidatore.GrigliaCoerenteConDimensioni);
        anagramma.Regole.Stato.Should().Be(StatoRegole.Approvata);
        anagramma.Regole.ApprovateIl.Should().Be(new DateOnly(2026, 11, 20));
        anagramma.Regole.ApprovateDa.Should().Be("proprietario");
        anagramma.CampoConRuolo(RuoloCampo.Diagramma)!.Max.Should().Be(100);
    }

    [Theory]
    [InlineData("non-valido-default.json")]
    [InlineData("non-valido-identificatore.json")]
    [InlineData("non-valido-parola-riservata.json")]
    [InlineData("non-valido-regole-senza-data.json")]
    [InlineData("non-valido-resa-senza-colonne.json")]
    [InlineData("non-valido-tipo.json")]
    public void Rifiuta_gli_esempi_non_validi_con_un_messaggio(string nome)
    {
        var errori = Errori(PercorsiTipologie.Esempio("descrittore", nome));

        errori.Should().NotBeEmpty().And.OnlyContain(e => e.Length > 0);
    }

    [Fact]
    public void Rifiuta_un_json_non_ben_formato()
    {
        Errori("{ non json").Should().ContainSingle().Which.Should().StartWith("JSON non valido");
    }

    [Fact]
    public void Carica_ogni_descrittore_della_cartella_comprese_le_versioni_0()
    {
        var catalogo = Caricatore.CaricaCartella(PercorsiTipologie.Cartella);

        catalogo.Descrittori.Where(d => d.EPrototipo).Select(d => d.Id)
            .Should().Contain(["sudoku", "futoshiki", "nonogramma", "rebus", "crittografia-breve"]);
        catalogo.Descrittori.Should().Contain(d => d.Id == "cruciverba-classico" && d.Versione == 1);
        catalogo.PerEstrazione().Should().OnlyContain(d => !d.EPrototipo);
        catalogo.Descrittori.Select(d => (d.Id, d.Versione)).Should().BeInAscendingOrder();
    }

    [Fact]
    public void Le_risorse_incorporate_coincidono_con_i_file_della_cartella()
    {
        var dalleRisorse = Caricatore.CaricaRisorse().Descrittori.Select(d => (d.Id, d.Versione));
        var dallaCartella = Caricatore.CaricaCartella(PercorsiTipologie.Cartella).Descrittori.Select(d => (d.Id, d.Versione));

        dalleRisorse.Should().Equal(dallaCartella);
    }

    [Fact]
    public void Un_prototipo_si_accetta_ma_non_ha_sql()
    {
        var catalogo = Caricatore.CaricaCartella(PercorsiTipologie.Cartella);

        catalogo.ConSql().Should().OnlyContain(d => d.Versione >= 1);
        catalogo.ConSql().Should().NotContain(d => d.Id == "sudoku");
    }

    [Fact]
    public void Controllo1_rifiuta_nomi_ripetuti_fra_campi_ed_elenchi()
    {
        var json = Modificato(n => n["elenchi"]![0]!["nome"] = "autore");

        Errori(json).Should().Contain(e => e.Contains("'autore'") && e.Contains("più volte"));
    }

    [Fact]
    public void Controllo1_rifiuta_nomi_ripetuti_nei_campi_di_un_elenco()
    {
        var json = Modificato(n => n["elenchi"]![0]!["campi"]![1]!["nome"] = "numero");

        Errori(json).Should().Contain(e => e.Contains("'numero'") && e.Contains("'definizioni'"));
    }

    [Fact]
    public void Controllo2_rifiuta_un_ruolo_ripetuto_nei_campi_del_Gioco()
    {
        var json = Modificato(n => n["campi"]!.AsArray().Add(new JsonObject { ["nome"] = "altre_righe", ["tipo"] = "intero", ["ruolo"] = "righe" }));

        Errori(json).Should().Contain(e => e.Contains("'righe'") && e.Contains("più volte"));
    }

    [Fact]
    public void Controllo3_rifiuta_id_e_versione_diversi_da_cartella_e_file()
    {
        var radice = Path.Combine(Path.GetTempPath(), "tipologie-" + Guid.NewGuid().ToString("N"));
        var cartella = Directory.CreateDirectory(Path.Combine(radice, "altro-nome")).FullName;
        try
        {
            File.WriteAllText(Path.Combine(cartella, "v2.json"), Cruciverba());

            var azione = () => Caricatore.CaricaFile(Path.Combine(cartella, "v2.json"));

            var errori = azione.Should().Throw<DescrittoreNonValidoException>().Which.Errori;
            errori.Should().Contain(e => e.Contains("'esempio-cruciverba'") && e.Contains("'altro-nome'"));
            errori.Should().Contain(e => e.Contains("versione 1") && e.Contains("v2.json"));
        }
        finally
        {
            Directory.Delete(radice, true);
        }
    }

    [Fact]
    public void Controllo3_rifiuta_un_nome_di_file_che_non_e_una_versione()
    {
        var radice = Path.Combine(Path.GetTempPath(), "tipologie-" + Guid.NewGuid().ToString("N"));
        var cartella = Directory.CreateDirectory(Path.Combine(radice, "esempio-cruciverba")).FullName;
        try
        {
            File.WriteAllText(Path.Combine(cartella, "bozza.json"), Cruciverba());

            var azione = () => Caricatore.CaricaFile(Path.Combine(cartella, "bozza.json"));

            azione.Should().Throw<DescrittoreNonValidoException>().Which.Errori.Should().Contain(e => e.Contains("v<versione>.json"));
        }
        finally
        {
            Directory.Delete(radice, true);
        }
    }

    [Fact]
    public void Controllo4_ai_limiti_del_meta_schema_i_nomi_di_tabella_restano_entro_63_byte()
    {
        var lungo = new string('a', 30);
        var json = Modificato(n =>
        {
            n["id"] = lungo;
            n["elenchi"]![0]!["nome"] = new string('b', 20);
        });

        Caricatore.CaricaTesto(json).Id.Should().Be(lungo);
    }

    private sealed class ValidatoreFinto(NomeValidatore nome, params RuoloCampo[] ruoli) : IValidatoreStrutturale
    {
        public NomeValidatore Nome { get; } = nome;

        public IReadOnlyList<RuoloCampo> RuoliRichiesti { get; } = ruoli;

        public EsitoValidazione Valida(Descrittore descrittore, System.Text.Json.JsonElement dati) => EsitoValidazione.Valido;
    }

    [Fact]
    public void Controllo5_rifiuta_un_validatore_che_richiede_un_ruolo_assente()
    {
        var caricatore = new CaricatoreDescrittori(
        [
            new ValidatoreFinto(NomeValidatore.GrigliaCoerenteConDimensioni, RuoloCampo.Righe, RuoloCampo.Griglia),
            new ValidatoreFinto(NomeValidatore.NumerazioneCoerenteConGriglia, RuoloCampo.Griglia, RuoloCampo.Numero, RuoloCampo.Direzione),
            new ValidatoreFinto(NomeValidatore.LunghezzeCoerenti, RuoloCampo.Soluzione, RuoloCampo.Esposto),
            new ValidatoreFinto(NomeValidatore.IncrociCoerenti, RuoloCampo.Griglia),
        ]);

        Errori(Cruciverba(), caricatore).Should().ContainSingle()
            .Which.Should().Contain("lunghezze-coerenti").And.Contain("'esposto'");
    }

    [Fact]
    public void Controllo5_rifiuta_un_validatore_senza_implementazione_e_accetta_quando_tutto_c_e()
    {
        var senzaNessuno = new CaricatoreDescrittori([]);
        Errori(Cruciverba(), senzaNessuno).Should().HaveCount(4).And.OnlyContain(e => e.Contains("non ha un'implementazione"));

        var completo = new CaricatoreDescrittori(
        [
            new ValidatoreFinto(NomeValidatore.GrigliaCoerenteConDimensioni, RuoloCampo.Griglia),
            new ValidatoreFinto(NomeValidatore.NumerazioneCoerenteConGriglia, RuoloCampo.Numero),
            new ValidatoreFinto(NomeValidatore.LunghezzeCoerenti, RuoloCampo.Soluzione),
            new ValidatoreFinto(NomeValidatore.IncrociCoerenti, RuoloCampo.Griglia),
        ]);
        completo.CaricaTesto(Cruciverba()).Id.Should().Be("esempio-cruciverba");
    }

    [Fact]
    public void Un_catalogo_rifiuta_la_stessa_coppia_id_versione_due_volte()
    {
        var d = PercorsiTipologie.DescrittoreEsempio("valido-esempio-anagramma.v1.json");

        var azione = () => CatalogoTipologie.Da([d, d]);

        azione.Should().Throw<ArgumentException>();
    }
}
