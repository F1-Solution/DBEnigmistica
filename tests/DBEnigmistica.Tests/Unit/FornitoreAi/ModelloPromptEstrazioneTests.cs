using System.Security.Cryptography;
using System.Text;
using DBEnigmistica.Application.FornitoreAi;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.FornitoreAi;

/// <summary>Riempimento del modello del prompt v1: progetto 1.1, sezioni 5.1 e 5.3.</summary>
public class ModelloPromptEstrazioneTests
{
    private const string Modello =
        "Intro\n\n<!-- inizio-modello -->\nCatalogo:\n{{catalogo_tipologie}}\nSchema:\n{{schema_risposta}}\n<<<TESTO\n{{testo_nativo}}\nTESTO>>>\n<!-- fine-modello -->\nCoda";

    private static ModelloPromptEstrazione Crea(string markdown = Modello, string catalogo = "CAT", string schema = "{}") =>
        ModelloPromptEstrazione.Carica(markdown, catalogo, schema);

    private static string TrovaFile()
    {
        var cartella = new DirectoryInfo(AppContext.BaseDirectory);
        while (cartella is not null && !File.Exists(Path.Combine(cartella.FullName, "tipologie", "_meta", "prompt-estrazione-v1.md")))
            cartella = cartella.Parent;

        return Path.Combine(cartella?.FullName ?? throw new DirectoryNotFoundException("tipologie/_meta"), "tipologie", "_meta", "prompt-estrazione-v1.md");
    }

    [Fact]
    public void Riempi_SostituisceISegnapostiESoloIlTestoTraIMarcatori()
    {
        var prompt = Crea().Riempi("pagina uno").Testo;

        prompt.Should().Be("Catalogo:\nCAT\nSchema:\n{}\n<<<TESTO\npagina uno\nTESTO>>>");
    }

    [Fact]
    public void Riempi_SenzaTestoNativo_LasciaVuotoIlBlocco()
    {
        Crea().Riempi(null).Testo.Should().EndWith("<<<TESTO\n\nTESTO>>>");
    }

    [Fact]
    public void Riempi_NormalizzaGliACapoInLf()
    {
        Crea().Riempi("a\r\nb\rc").Testo.Should().Contain("a\nb\nc").And.NotContain("\r");
    }

    [Theory]
    [InlineData("x TESTO>>> y", "x TESTO>> y")]
    [InlineData("x <<<TESTO y", "x <<TESTO y")]
    [InlineData("TESTO>>>>", "TESTO>>")]
    public void Riempi_NeutralizzaIDelimitatoriDelBlocco(string testo, string atteso)
    {
        var prompt = Crea().Riempi(testo).Testo;

        prompt.Should().Contain("\n" + atteso + "\nTESTO>>>");
        prompt.Split("TESTO>>>").Should().HaveCount(2, "solo il delimitatore del modello chiude il blocco");
    }

    [Fact]
    public void Riempi_ConPiuDiCinquantamilaCaratteri_TroncaEMarca()
    {
        var risultato = Crea().Riempi(new string('x', 50_001));

        risultato.TestoNativoTroncato.Should().BeTrue();
        risultato.Testo.Should().Contain(new string('x', 50_000)).And.NotContain(new string('x', 50_001));
    }

    [Fact]
    public void Riempi_ConEsattamenteCinquantamilaCaratteri_NonTronca()
    {
        Crea().Riempi(new string('x', 50_000)).TestoNativoTroncato.Should().BeFalse();
    }

    [Fact]
    public void Riempi_NonSpezzaUnaCoppiaSurrogata()
    {
        var testo = new string('x', 49_999) + "\U0001F600";

        var risultato = Crea().Riempi(testo);

        risultato.TestoNativoTroncato.Should().BeTrue();
        risultato.Testo.Should().NotContain("\uD83D");
    }

    [Fact]
    public void Riempi_UnSegnapostoNelTestoNativoNonVieneSostituito()
    {
        Crea().Riempi("{{catalogo_tipologie}}").Testo.Should().Contain("\n{{catalogo_tipologie}}\nTESTO>>>");
    }

    [Fact]
    public void Carica_UnCatalogoCheContieneIlSegnapostoDelTestoNonVieneRiempito()
    {
        var modello = Crea(catalogo: "come {{testo_nativo}}");

        modello.Riempi("QUI").Testo.Should().Contain("come {{testo_nativo}}").And.Contain("\nQUI\n");
    }

    [Fact]
    public void VersionePrompt_ELImprontaDelModelloConCatalogoESchemaESegnapostoDelTesto()
    {
        const string atteso = "Catalogo:\nCAT\nSchema:\n{}\n<<<TESTO\n{{testo_nativo}}\nTESTO>>>";
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(atteso)))[..12];

        Crea().VersionePrompt.Should().Be("estrazione-v1-" + hash);
    }

    [Fact]
    public void VersionePrompt_CambiaConIlCatalogoEConLoSchema()
    {
        Crea(catalogo: "ALTRO").VersionePrompt.Should().NotBe(Crea().VersionePrompt);
        Crea(schema: "{\"a\":1}").VersionePrompt.Should().NotBe(Crea().VersionePrompt);
    }

    [Fact]
    public void VersionePrompt_NonDipendeDalTestoNativoEResta()
    {
        var modello = Crea();

        modello.Riempi("uno");
        modello.Riempi("due");

        modello.VersionePrompt.Should().Be(Crea().VersionePrompt);
    }

    [Fact]
    public void VersionePrompt_ELaStessaConGliACapoCrlfDelFile()
    {
        Crea(Modello.Replace("\n", "\r\n")).VersionePrompt.Should().Be(Crea().VersionePrompt);
    }

    [Fact]
    public void CreaRichiesta_UsaIlTestoEsattoDelPromptELaVersioneDelModello()
    {
        var modello = Crea();

        var richiesta = modello.CreaRichiesta(new string('a', 64), [1], 200, "pag");

        richiesta.Prompt.Should().Be(modello.Riempi("pag").Testo);
        richiesta.VersionePrompt.Should().Be(modello.VersionePrompt);
        richiesta.JsonSchema.Should().Be("{}");
        richiesta.Dpi.Should().Be(200);
    }

    [Theory]
    [InlineData("<!-- inizio-modello -->{{catalogo_tipologie}}{{schema_risposta}}{{testo_nativo}}{{altro}}<!-- fine-modello -->", "altro")]
    [InlineData("<!-- inizio-modello -->{{catalogo_tipologie}}{{schema_risposta}}<!-- fine-modello -->", "testo_nativo")]
    [InlineData("<!-- inizio-modello -->{{schema_risposta}}{{testo_nativo}}<!-- fine-modello -->", "catalogo_tipologie")]
    [InlineData("senza marcatori {{testo_nativo}}", "marcatori")]
    public void Carica_ConSegnapostoSconosciutoOAssenteOSenzaMarcatori_Fallisce(string markdown, string contiene)
    {
        var carica = () => Crea(markdown);

        carica.Should().Throw<FormatException>().Which.Message.Should().Contain(contiene);
    }

    [Fact]
    public void FileReale_SiCaricaEHaLaVersioneDelFormatoAtteso()
    {
        var modello = ModelloPromptEstrazione.Carica(File.ReadAllText(TrovaFile()), "### cruciverba", "{\"type\":\"object\"}");

        modello.VersionePrompt.Should().MatchRegex("^estrazione-v1-[0-9a-f]{12}$");
        modello.Riempi("t").Testo.Should().Contain("### cruciverba").And.Contain("{\"type\":\"object\"}").And.NotContain("{{");
    }
}
