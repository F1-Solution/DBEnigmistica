using DBEnigmistica.AI.Services;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.FornitoreAi;

/// <summary>Il validatore applica davvero lo schema della risposta per pagina (draft 2020-12).</summary>
public class ValidatoreSchemaJsonTests
{
    private static readonly string Meta = TrovaMeta();

    private static string Schema => File.ReadAllText(Path.Combine(Meta, "risposta-pagina.schema.json"));

    public static TheoryData<string> Validi => Elenca("valido-");

    public static TheoryData<string> NonValidi => Elenca("non-valido-");

    private static string TrovaMeta()
    {
        var cartella = new DirectoryInfo(AppContext.BaseDirectory);
        while (cartella is not null && !Directory.Exists(Path.Combine(cartella.FullName, "tipologie", "_meta")))
            cartella = cartella.Parent;

        return Path.Combine(cartella?.FullName ?? throw new DirectoryNotFoundException("tipologie/_meta"), "tipologie", "_meta");
    }

    private static TheoryData<string> Elenca(string prefisso)
    {
        var dati = new TheoryData<string>();
        foreach (var file in Directory.GetFiles(Path.Combine(TrovaMeta(), "esempi", "risposta-pagina"), prefisso + "*.json").Order())
            dati.Add(Path.GetFileName(file));

        return dati;
    }

    private static string Esempio(string nome) => File.ReadAllText(Path.Combine(Meta, "esempi", "risposta-pagina", nome));

    [Theory]
    [MemberData(nameof(Validi))]
    public void EsempiValidi_Passano(string nome) =>
        new ValidatoreSchemaJson().Valida(Esempio(nome), Schema).Should().BeEmpty();

    [Theory]
    [MemberData(nameof(NonValidi))]
    public void EsempiNonValidi_Falliscono(string nome) =>
        new ValidatoreSchemaJson().Valida(Esempio(nome), SchemaPer(nome)).Should().NotBeEmpty();

    // Questo esempio è sbagliato solo rispetto allo schema derivato dai descrittori.
    private static string SchemaPer(string nome) =>
        nome == "non-valido-solo-derivato-dati.json"
            ? File.ReadAllText(Path.Combine(Meta, "esempi", "risposta-pagina", "derivato-esempio.schema.json"))
            : Schema;

    [Fact]
    public void EsempioValido_PassaAncheLoSchemaDerivato() =>
        new ValidatoreSchemaJson().Valida(Esempio("valido-s01-cruciverba.json"), SchemaPer("non-valido-solo-derivato-dati.json")).Should().BeEmpty();

    [Fact]
    public void JsonMalformato_DaUnErrore() =>
        new ValidatoreSchemaJson().Valida("{non json", Schema).Should().ContainSingle().Which.Should().Contain("JSON non valido");

    [Fact]
    public void StessoSchemaPiuVolte_SiComportaInModoCoerente()
    {
        var validatore = new ValidatoreSchemaJson();
        const string schema = "{\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"type\":\"object\",\"required\":[\"a\"]}";

        validatore.Valida("{\"a\":1}", schema).Should().BeEmpty();
        validatore.Valida("{}", schema).Should().NotBeEmpty();
        validatore.Valida("{\"a\":1}", schema).Should().BeEmpty();
    }

    private const string SchemaAnnidato =
        "{\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"type\":\"object\",\"required\":[\"a\"],"
        + "\"properties\":{\"b\":{\"type\":\"integer\"},\"d\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"required\":[\"x\"]}}}}";

    [Fact]
    public void Messaggi_ContengonoPosizioneEMotivo()
    {
        var errori = new ValidatoreSchemaJson().Valida("{\"b\":\"no\",\"d\":[{}]}", SchemaAnnidato);

        errori.Should().Contain(e => e.StartsWith("/b:") && e.Contains("integer"));
        errori.Should().Contain(e => e.StartsWith("/d/0/x:") && e.Contains("Required property not present 'x'"));
        errori.Should().Contain(e => e.StartsWith("/a:") && e.Contains("'a'"));
    }

    [Fact]
    public void Messaggi_NonHannoVociVuoteGenericheODuplicate()
    {
        var errori = new ValidatoreSchemaJson().Valida("{\"b\":\"no\",\"d\":[{},{}]}", SchemaAnnidato);

        errori.Should().OnlyContain(e => !e.EndsWith(": ") && !e.Contains("match the subschema"));
        errori.Should().OnlyHaveUniqueItems();
    }
}
