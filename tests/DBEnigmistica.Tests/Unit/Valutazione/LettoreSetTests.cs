using DBEnigmistica.Application.Valutazione;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Valutazione;

public class LettoreSetTests
{
    private const string Cartella = "C:\\set-di-prova";

    private static string Json(string tipo = "sintetico", string? generatore = "set-sintetico-v1", string id = "s01", string? pdf = null, int? numeroPagina = 1)
    {
        var gen = generatore is null ? string.Empty : $"\"generatore\": \"{generatore}\",";
        var pdfJson = pdf is null ? "null" : $"\"{pdf.Replace("\\", "\\\\", StringComparison.Ordinal)}\"";
        var numero = numeroPagina is null ? "null" : numeroPagina.ToString();
        return $$"""
            { "versione_formato": 1, "nome": "prova", "tipo": "{{tipo}}", {{gen}}
              "pagine": [ { "id": "{{id}}", "origine": "digitale", "pdf": {{pdfJson}}, "numero_pagina": {{numero}} } ] }
            """;
    }

    [Fact]
    public void Interpreta_SetSinteticoConGeneratore_LeggeLePagine()
    {
        var set = LettoreSet.Interpreta(Cartella, Json());

        set.Tipo.Should().Be(TipoSet.Sintetico);
        set.Generatore.Should().Be("set-sintetico-v1");
        set.Pagine.Should().ContainSingle().Which.Should().Be(new PaginaSet("s01", "digitale", null, 1));
    }

    [Theory]
    [InlineData("S01")]
    [InlineData("../s01")]
    [InlineData("s 01")]
    [InlineData("-s01")]
    [InlineData("")]
    [InlineData("a123456789012345678901234567890123456789012345678901234567890123456")]
    public void Interpreta_IdFuoriForma_Rifiutato(string id)
    {
        var azione = () => LettoreSet.Interpreta(Cartella, Json(id: id));

        azione.Should().Throw<SetNonValidoException>();
    }

    [Theory]
    [InlineData("../esterno.pdf")]
    [InlineData("pdf/../../esterno.pdf")]
    [InlineData("pdf\\..\\..\\esterno.pdf")]
    [InlineData("C:\\esterno.pdf")]
    [InlineData("C:esterno.pdf")]
    [InlineData("/etc/esterno.pdf")]
    [InlineData("\\\\server\\condivisa\\esterno.pdf")]
    [InlineData("")]
    public void Interpreta_PercorsoPdfConPuntiPuntiAssolutoOEsterno_Rifiutato(string pdf)
    {
        var azione = () => LettoreSet.Interpreta(Cartella, Json(tipo: "riferimento", generatore: null, pdf: pdf));

        azione.Should().Throw<SetNonValidoException>();
    }

    [Fact]
    public void Interpreta_PercorsoRelativoInterno_Accettato()
    {
        var set = LettoreSet.Interpreta(Cartella, Json(tipo: "riferimento", generatore: null, pdf: "pdf/rivista-prova.pdf"));

        LettoreSet.PercorsoPdf(set, set.Pagine[0]).Should().Be(Path.GetFullPath(Path.Combine(Cartella, "pdf", "rivista-prova.pdf")));
    }

    [Fact]
    public void Interpreta_GeneratoreConTipoRiferimento_Rifiutato()
    {
        var azione = () => LettoreSet.Interpreta(Cartella, Json(tipo: "riferimento"));

        azione.Should().Throw<SetNonValidoException>().WithMessage("*generatore*");
    }

    [Theory]
    [InlineData("Set Sintetico")]
    [InlineData("../altro")]
    [InlineData("")]
    public void Interpreta_NomeDelGeneratoreFuoriForma_Rifiutato(string generatore)
    {
        var azione = () => LettoreSet.Interpreta(Cartella, Json(generatore: generatore));

        azione.Should().Throw<SetNonValidoException>();
    }

    [Fact]
    public void Interpreta_PaginaSenzaPdfNeGeneratore_Rifiutata()
    {
        var azione = () => LettoreSet.Interpreta(Cartella, Json(tipo: "riferimento", generatore: null));

        azione.Should().Throw<SetNonValidoException>().WithMessage("*s01*");
    }

    [Fact]
    public void Interpreta_IdDuplicati_Rifiutati()
    {
        var json = """
            { "versione_formato": 1, "nome": "prova", "tipo": "sintetico", "generatore": "set-sintetico-v1",
              "pagine": [ { "id": "s01", "origine": "digitale", "pdf": null, "numero_pagina": 1 },
                          { "id": "s01", "origine": "scansione", "pdf": null, "numero_pagina": 2 } ] }
            """;

        var azione = () => LettoreSet.Interpreta(Cartella, json);

        azione.Should().Throw<SetNonValidoException>().WithMessage("*s01*");
    }

    [Theory]
    [InlineData("{ \"versione_formato\": 2, \"nome\": \"p\", \"tipo\": \"sintetico\", \"pagine\": [] }")]
    [InlineData("{ \"versione_formato\": 1, \"nome\": \"p\", \"tipo\": \"altro\", \"pagine\": [] }")]
    [InlineData("{ \"versione_formato\": 1, \"nome\": \"p\", \"tipo\": \"sintetico\", \"pagine\": [] }")]
    [InlineData("non e' json")]
    public void Interpreta_FormatoNonValido_Rifiutato(string json)
    {
        var azione = () => LettoreSet.Interpreta(Cartella, json);

        azione.Should().Throw<SetNonValidoException>();
    }

    [Fact]
    public void Leggi_CartellaSenzaSetJson_ErroreEsplicito()
    {
        var azione = () => LettoreSet.Leggi(Path.Combine(Path.GetTempPath(), "dbe-non-esiste-" + Guid.NewGuid().ToString("N")));

        azione.Should().Throw<SetNonValidoException>().WithMessage("*set.json*");
    }
}
