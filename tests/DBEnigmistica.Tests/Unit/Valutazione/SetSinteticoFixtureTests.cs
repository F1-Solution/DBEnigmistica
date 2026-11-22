using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.Valutazione;
using DBEnigmistica.Tests.Unit.Creazione;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Valutazione;

public class SetSinteticoFixtureTests
{
    private static readonly string[] Pagine = ["s01", "s02", "s03", "s04", "s05", "s06"];

    private static string Fixture(params string[] parti) =>
        File.ReadAllText(FileDelRepository.Percorso(["tests", "DBEnigmistica.Tests", "Fixtures", "SetSintetico", .. parti]));

    [Fact]
    public void SetJson_ElencaS01S06ConGeneratoreESenzaPdf()
    {
        var set = LettoreSet.Leggi(FileDelRepository.Percorso("tests", "DBEnigmistica.Tests", "Fixtures", "SetSintetico"));

        set.Tipo.Should().Be(TipoSet.Sintetico);
        set.Generatore.Should().Be("set-sintetico-v1");
        set.Pagine.Select(p => p.Id).Should().Equal(Pagine);
        set.Pagine.Should().OnlyContain(p => p.Pdf == null);
        set.Pagine.Single(p => p.Id == "s04").Origine.Should().Be("scansione");
    }

    [Theory]
    [InlineData("s01")]
    [InlineData("s02")]
    [InlineData("s03")]
    [InlineData("s04")]
    [InlineData("s05")]
    [InlineData("s06")]
    public void RisposteEAttesi_SonoConformiAgliSchemiDelMetaSchema(string id)
    {
        var validatore = new ValidatoreSchemaJson();
        var schemaRisposta = FileDelRepository.Leggi("tipologie", "_meta", "risposta-pagina.schema.json");
        var schemaAtteso = FileDelRepository.Leggi("tipologie", "_meta", "risultato-atteso.schema.json");

        validatore.Valida(Fixture("risposte", id + ".json"), schemaRisposta).Should().BeEmpty();
        validatore.Valida(Fixture("attesi", id + ".json"), schemaAtteso).Should().BeEmpty();
    }

    [Theory]
    [InlineData("s01")]
    [InlineData("s02")]
    [InlineData("s03")]
    [InlineData("s04")]
    [InlineData("s05")]
    [InlineData("s06")]
    public void Risposte_SonoConformiAlloSchemaDerivatoDalCatalogo(string id)
    {
        new ValidatoreSchemaJson().Valida(Fixture("risposte", id + ".json"), ServiziDiProva.Versione.JsonSchema).Should().BeEmpty();
    }
}
