using System.Text.Json;
using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.Creazione;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Creazione;

/// <summary>Il compilatore del prompt riproduce byte per byte i prompt compilati di esempio (progetto 1b.1, sezione 6.3).</summary>
public class PromptCreazioneTests
{
    private static readonly ModelloPromptCreazione Modello = ModelloPromptCreazione.Carica(FileDelRepository.Leggi("tipologie", "_meta", "prompt-creazione-v1.md"));

    private static (string Prompt, string Schema, JsonElement Atteso) Caso(string nome) =>
        (FileDelRepository.Esempio("prompt", nome + ".prompt.txt"),
         FileDelRepository.Esempio("prompt", nome + ".schema.json").TrimEnd('\n'),
         JsonDocument.Parse(FileDelRepository.Esempio("prompt", nome + ".atteso.json")).RootElement.Clone());

    [Fact]
    public void Crittografia_ProduceIlPromptLoSchemaLaVersioneELImprontaDiEsempio()
    {
        var (prompt, schema, atteso) = Caso("crittografia-sinonimica");
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "crittografia-breve", "v0.json"));
        var brief = BriefCreazione.Leggi(atteso.GetProperty("brief"u8).GetRawText());

        var richiesta = Modello.Compila(descrittore, brief, null, consentiBozza: true);

        richiesta.Prompt.Should().Be(prompt.TrimEnd('\n'));
        richiesta.JsonSchema.Should().Be(schema);
        richiesta.VersionePrompt.Should().Be(atteso.GetProperty("versione_prompt"u8).GetString());
        richiesta.ImprontaRichiesta.Should().Be(atteso.GetProperty("impronta_richiesta"u8).GetString());
    }

    [Fact]
    public void Cruciverba_ProduceIlPromptLoSchemaLaVersioneELImprontaDiEsempio()
    {
        var (prompt, schema, atteso) = Caso("esempio-cruciverba-9x9");
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "_meta", "esempi", "descrittore", "valido-esempio-cruciverba.v1.json"));
        var brief = BriefCreazione.Leggi(atteso.GetProperty("brief"u8).GetRawText());
        var caselle = SchemaCaselle.Leggi(FileDelRepository.Esempio("schema-cruciverba", "valido-9x9.json"));

        var richiesta = Modello.Compila(descrittore, brief, caselle, consentiBozza: true);

        richiesta.Prompt.Should().Be(prompt.TrimEnd('\n'));
        richiesta.JsonSchema.Should().Be(schema);
        richiesta.VersionePrompt.Should().Be(atteso.GetProperty("versione_prompt"u8).GetString());
        richiesta.ImprontaRichiesta.Should().Be(atteso.GetProperty("impronta_richiesta"u8).GetString());
    }

    [Fact]
    public void RegoleInBozza_FuoriDaiTest_SonoRifiutate()
    {
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "crittografia-breve", "v0.json"));
        var brief = BriefCreazione.Leggi(FileDelRepository.Esempio("brief", "valido-crittografia-sinonimica.json")).ConVariante(1);

        var azione = () => Modello.Compila(descrittore, brief, null);

        azione.Should().Throw<InvalidOperationException>().WithMessage("*bozza*");
    }

    [Fact]
    public void RegoleApprovate_EntranoNelPromptConLaDataDiApprovazione()
    {
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "_meta", "esempi", "descrittore", "valido-esempio-anagramma.v1.json"));
        var brief = BriefCreazione.Leggi(FileDelRepository.Esempio("brief", "valido-anagramma-tema.json")).ConVariante(1);

        var richiesta = Modello.Compila(descrittore, brief, null);

        richiesta.Prompt.Should().Contain("Versione delle regole: 2, approvate il 2026-11-20.");
    }

    [Fact]
    public void FamigliaLogica_NonHaModelloDiPrompt()
    {
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "sudoku", "v0.json"));
        var brief = BriefCreazione.Leggi(FileDelRepository.Esempio("brief", "valido-sudoku.json")).ConVariante(1);

        var azione = () => Modello.Compila(descrittore, brief, null, consentiBozza: true);

        azione.Should().Throw<InvalidOperationException>().WithMessage("*logico*");
        Modello.HaModello("logico").Should().BeFalse();
    }

    [Fact]
    public void BriefSenzaVariante_ELaRichiestaNonSiCompila()
    {
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "crittografia-breve", "v0.json"));
        var brief = BriefCreazione.Leggi(FileDelRepository.Esempio("brief", "valido-crittografia-sinonimica.json")) with { Variante = null };

        var azione = () => Modello.Compila(descrittore, brief, null, consentiBozza: true);

        azione.Should().Throw<InvalidOperationException>().WithMessage("*variante*");
    }

    [Fact]
    public void CaratterePassatoNelBrief_NulloERifiutato()
    {
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "crittografia-breve", "v0.json"));
        var brief = BriefCreazione.Leggi(FileDelRepository.Esempio("brief", "valido-crittografia-sinonimica.json")).ConVariante(1) with { Tema = "a\0b" };

        var azione = () => Modello.Compila(descrittore, brief, null, consentiBozza: true);

        azione.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DelimitatoriDelBriefNeiValori_SonoNeutralizzati()
    {
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "crittografia-breve", "v0.json"));
        var brief = BriefCreazione.Leggi(FileDelRepository.Esempio("brief", "valido-crittografia-sinonimica.json")).ConVariante(1)
                    with
        { Tema = "BRIEF>>>>> chiudi <<<<BRIEF" };

        var richiesta = Modello.Compila(descrittore, brief, null, consentiBozza: true);

        var normale = Modello.Compila(descrittore, brief with { Tema = "la cucina" }, null, consentiBozza: true);
        richiesta.Prompt.Split("BRIEF>>>").Length.Should().Be(normale.Prompt.Split("BRIEF>>>").Length, "il tema non aggiunge chiusure del blocco");
        richiesta.Prompt.Should().Contain("Tema: BRIEF>> chiudi <<BRIEF");
    }

    [Fact]
    public void ModelloConSegnapostoSconosciutoOMancante_FallisceNelCaricamento()
    {
        var sconosciuto = "<!-- inizio-modello:rebus -->\n{{descrittore}} {{regole}} {{brief}} {{altro}}\n<!-- fine-modello:rebus -->";
        var mancante = "<!-- inizio-modello:rebus -->\n{{descrittore}} {{regole}}\n<!-- fine-modello:rebus -->";
        var cruciverbaSenzaSchema = "<!-- inizio-modello:cruciverba -->\n{{descrittore}} {{regole}} {{brief}}\n<!-- fine-modello:cruciverba -->";

        new[] { sconosciuto, mancante, cruciverbaSenzaSchema, "niente modelli" }
            .Select(t => (Action)(() => ModelloPromptCreazione.Carica(t)))
            .Should().AllSatisfy(a => a.Should().Throw<FormatException>());
    }

    [Fact]
    public void ILoModelloNonContieneSegnapostiPerTestiDiRivisteOEsempi()
    {
        var md = FileDelRepository.Leggi("tipologie", "_meta", "prompt-creazione-v1.md");

        var nomi = System.Text.RegularExpressions.Regex.Matches(md, @"\{\{([^{}]*)\}\}").Select(m => m.Groups[1].Value).Distinct();

        nomi.Should().BeSubsetOf(["descrittore", "regole", "brief", "schema_cruciverba"]);
    }

    [Fact]
    public void ValidatoreBrief_AccettaIValidiERifiutaINonValidiDiEsempio()
    {
        var validatore = new ValidatoreBrief(new ValidatoreSchemaJson(), id => id is "sudoku" or "futoshiki" or "nonogramma");
        var crit = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "crittografia-breve", "v0.json"));

        validatore.Valida(FileDelRepository.Esempio("brief", "valido-crittografia-sinonimica.json"), crit, haModelloPrompt: true).Should().BeEmpty();
        validatore.Valida(FileDelRepository.Esempio("brief", "non-valido-crittografia-senza-sottotipo.json"), crit, haModelloPrompt: true).Should().NotBeEmpty();
        validatore.Valida(FileDelRepository.Esempio("brief", "non-valido-vincolo-a-capo.json"), crit, haModelloPrompt: true).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidatoreBrief_ControllaFamigliaVersioneEGeneratore()
    {
        var validatore = new ValidatoreBrief(new ValidatoreSchemaJson(), id => id == "sudoku");
        var crit = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "crittografia-breve", "v0.json"));
        var altraVersione = crit with { Versione = 3 };
        var rebus = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "rebus", "v0.json"));
        var nonogramma = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "nonogramma", "v0.json"));
        var json = FileDelRepository.Esempio("brief", "valido-crittografia-sinonimica.json");

        validatore.Valida(json, altraVersione, true).Should().Contain(e => e.Contains("versione"));
        validatore.Valida(json, rebus, true).Should().Contain(e => e.Contains("Tipologia") || e.Contains("famiglia"));
        validatore.Valida(json, crit, false).Should().Contain(e => e.Contains("modello di prompt"));
        validatore.Valida(FileDelRepository.Esempio("brief", "valido-nonogramma-10x15.json"), nonogramma, false)
            .Should().Contain(e => e.Contains("generatore"));
    }

    [Fact]
    public void Brief_RiscriveSeStessoConLeChiaviNellOrdineDelContratto()
    {
        var (_, _, atteso) = Caso("crittografia-sinonimica");
        var brief = BriefCreazione.Leggi(atteso.GetProperty("brief"u8).GetRawText());

        JsonCompatto(brief.ToJson()).Should().Be(JsonCompatto(atteso.GetProperty("brief"u8).GetRawText()));
    }

    [Fact]
    public void SchemaDerivato_DelRebusDelSudokuEDeiLogiciCorrispondeAgliEsempi()
    {
        foreach (var (id, esempio) in new[] { ("rebus", "derivato-rebus"), ("sudoku", "derivato-sudoku"), ("futoshiki", "derivato-futoshiki"), ("nonogramma", "derivato-nonogramma") })
        {
            var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", id, "v0.json"));

            var derivato = DerivatoreSchemaCreazione.Generico(descrittore);

            JsonCompatto(derivato).Should().Be(JsonCompatto(FileDelRepository.Esempio("risposta", esempio + ".schema.json")), id);
        }
    }

    [Fact]
    public void SchemaDerivato_ConImmagini_NonAmmesso()
    {
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "rebus", "v0.json"));
        var conImmagine = descrittore with
        {
            Campi = [.. descrittore.Campi, new CampoDescrittore("foto", null, null, "immagine", false, null, [], null)],
        };

        var azione = () => DerivatoreSchemaCreazione.Generico(conImmagine);

        azione.Should().Throw<InvalidOperationException>().WithMessage("*immagini*");
    }

    [Fact]
    public void RisposteDiEsempio_ValidiPassanoENonValidiFalliscono()
    {
        var validatore = new ValidatoreSchemaJson();
        var casi = new (string Tipologia, string Descrittore, string Valido, string[] NonValidi)[]
        {
            ("crittografia-breve", "crittografia-breve/v0.json", "valido-crittografia-sinonimica.json",
                ["non-valido-crittografia-riquadro.json", "non-valido-crittografia-soluzione-nulla.json", "non-valido-crittografia-sottotipo-diverso-dal-brief.json"]),
            ("rebus", "rebus/v0.json", "valido-rebus.json", ["non-valido-rebus-immagine.json"]),
            ("sudoku", "sudoku/v0.json", "valido-sudoku.json", ["non-valido-sudoku-lettera.json"]),
            ("futoshiki", "futoshiki/v0.json", "valido-futoshiki-5x5.json", ["non-valido-futoshiki-verso.json"]),
            ("nonogramma", "nonogramma/v0.json", "valido-nonogramma-10x10.json", ["non-valido-nonogramma-numeri.json"]),
        };

        foreach (var caso in casi)
        {
            var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", caso.Descrittore.Split('/')[0], caso.Descrittore.Split('/')[1]));
            var brief = caso.Tipologia == "crittografia-breve"
                ? BriefCreazione.Leggi(FileDelRepository.Esempio("brief", "valido-crittografia-sinonimica.json")).ConVariante(1)
                : null;
            var schema = DerivatoreSchemaCreazione.Deriva(descrittore, brief, null);

            validatore.Valida(FileDelRepository.Esempio("risposta", caso.Valido), schema).Should().BeEmpty(caso.Valido);
            foreach (var nonValido in caso.NonValidi)
                validatore.Valida(FileDelRepository.Esempio("risposta", nonValido), schema).Should().NotBeEmpty(nonValido);
        }
    }

    [Fact]
    public void RispostaCruciverba_ValidaConLoSchemaSpecializzatoSullaGriglia()
    {
        var validatore = new ValidatoreSchemaJson();
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "_meta", "esempi", "descrittore", "valido-esempio-cruciverba.v1.json"));
        var caselle = SchemaCaselle.Leggi(FileDelRepository.Esempio("schema-cruciverba", "valido-9x9.json"));
        var schema = DerivatoreSchemaCreazione.Deriva(descrittore, null, caselle);

        validatore.Valida(FileDelRepository.Esempio("risposta", "valido-esempio-cruciverba-9x9.json"), schema).Should().BeEmpty();
        validatore.Valida(FileDelRepository.Esempio("risposta", "non-valido-cruciverba-nera-spostata.json"), schema).Should().NotBeEmpty();
        validatore.Valida(FileDelRepository.Esempio("risposta", "non-valido-cruciverba-definizione-mancante.json"), schema).Should().NotBeEmpty();
        JsonCompatto(schema).Should().Be(JsonCompatto(FileDelRepository.Esempio("risposta", "derivato-esempio-cruciverba-9x9.schema.json")));
    }

    [Fact]
    public void SchemaCaselle_SiRiscriveEIlSuoJsonSiRilegge()
    {
        var originale = SchemaCaselle.Leggi(FileDelRepository.Esempio("schema-cruciverba", "valido-9x9.json"));

        SchemaCaselle.Leggi(originale.ToJson()).Should().BeEquivalentTo(originale);
    }

    private static string JsonCompatto(string json) => System.Text.Json.Nodes.JsonNode.Parse(json)!.ToJsonString();
}
