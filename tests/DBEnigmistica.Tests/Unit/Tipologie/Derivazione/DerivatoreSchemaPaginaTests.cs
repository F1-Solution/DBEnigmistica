using System.Text.Json.Nodes;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Derivazione;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Tipologie.Derivazione;

public class DerivatoreSchemaPaginaTests
{
    private static readonly ValidatoreSchema Validatore = new();

    private static string Derivato() => PercorsiTipologie.Esempio("risposta-pagina", "derivato-esempio.schema.json");

    [Fact]
    public void Lo_schema_derivato_dai_due_esempi_coincide_con_quello_di_riferimento()
    {
        var schema = DerivatoreSchemaPagina.Deriva(PercorsiTipologie.DueEsempi());

        JsonNode.DeepEquals(JsonNode.Parse(schema), JsonNode.Parse(Derivato())).Should().BeTrue();
    }

    [Fact]
    public void Il_testo_coincide_byte_per_byte_a_parte_l_a_capo_finale()
    {
        var schema = DerivatoreSchemaPagina.Deriva(PercorsiTipologie.DueEsempi());

        schema.Should().Be(Derivato().TrimEnd('\n'));
    }

    [Fact]
    public void Due_derivazioni_e_un_ordine_diverso_danno_lo_stesso_testo()
    {
        var uno = DerivatoreSchemaPagina.Deriva(PercorsiTipologie.DueEsempi());
        var due = DerivatoreSchemaPagina.Deriva(PercorsiTipologie.DueEsempi().Reverse());

        due.Should().Be(uno);
        uno.Should().NotContain("\r");
    }

    [Theory]
    [InlineData("valido-s01-cruciverba.json")]
    [InlineData("valido-s06-proposta.json")]
    public void Le_risposte_valide_di_esempio_passano_lo_schema_derivato(string nome)
    {
        var schema = DerivatoreSchemaPagina.Deriva(PercorsiTipologie.DueEsempi());

        Validatore.Valida(PercorsiTipologie.Esempio("risposta-pagina", nome), schema).Should().BeEmpty();
    }

    [Theory]
    [InlineData("non-valido-solo-derivato-dati.json")]
    [InlineData("non-valido-confidenza.json")]
    [InlineData("non-valido-riquadro.json")]
    public void Le_risposte_non_valide_non_passano_lo_schema_derivato(string nome)
    {
        var schema = DerivatoreSchemaPagina.Deriva(PercorsiTipologie.DueEsempi());

        Validatore.Valida(PercorsiTipologie.Esempio("risposta-pagina", nome), schema).Should().NotBeEmpty();
    }

    [Fact]
    public void Il_catalogo_reale_produce_uno_schema_che_accetta_un_anagramma_e_rifiuta_un_campo_in_piu()
    {
        var catalogo = new CaricatoreDescrittori().CaricaCartella(PercorsiTipologie.Cartella);
        var schema = DerivatoreSchemaPagina.Deriva(catalogo.PerEstrazione());

        string Risposta(string extra) => """
            {"versione_formato":1,"contenuto_pagina":"giochi","rivista":null,"avvisi":[],"giochi":[{
              "stato_tipologia":"nota","tipologia":"anagramma","versione_tipologia":1,"titolo":null,
              "riquadro":{"x":0.1,"y":0.1,"larghezza":0.5,"altezza":0.2},
              "dati":{"esposto":"Testo inventato","diagramma":"4 = 4","prima_parola":null,"soluzione":null,"autore":null@EXTRA@},
              "confidenza":{"gioco":0.9,"tipologia":0.9,"campi":{"esposto":0.9,"diagramma":0.9,"prima_parola":0.9,"soluzione":0.9,"autore":0.9},"elenchi":{}},
              "avvisi":[]}]}
            """.Replace("@EXTRA@", extra, StringComparison.Ordinal);

        Validatore.Valida(Risposta(""), schema).Should().BeEmpty();
        Validatore.Valida(Risposta(",\"estraneo\":1"), schema).Should().NotBeEmpty();
    }

    [Fact]
    public void Ogni_chiave_dei_dati_e_obbligatoria_e_un_campo_facoltativo_ammette_null()
    {
        var nodo = DerivatoreSchemaPagina.DerivaNodo(PercorsiTipologie.DueEsempi());

        var dati = nodo["$defs"]!["gioco_esempio_anagramma"]!["properties"]!["dati"]!;

        dati["required"]!.AsArray().Select(v => v!.GetValue<string>()).Should().Equal("esposto", "diagramma", "soluzione", "autore");
        dati["properties"]!["esposto"]!["type"]!.GetValue<string>().Should().Be("string");
        dati["properties"]!["autore"]!["anyOf"]![0]!["type"]!.GetValue<string>().Should().Be("null");
    }

    [Fact]
    public void Rifiuta_prototipi_e_Tipologie_con_piu_versioni()
    {
        var v0 = new CaricatoreDescrittori().CaricaCartella(PercorsiTipologie.Cartella).Descrittori.First(x => x.EPrototipo);
        var anagramma = PercorsiTipologie.DescrittoreEsempio("valido-esempio-anagramma.v1.json");

        var conPrototipo = () => DerivatoreSchemaPagina.Deriva([v0]);
        var conDoppio = () => DerivatoreSchemaPagina.Deriva([anagramma, anagramma with { Versione = 2 }]);

        conPrototipo.Should().Throw<ArgumentException>();
        conDoppio.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Il_nome_della_definizione_sostituisce_i_trattini()
    {
        DerivatoreSchemaPagina.NomeDefinizioneGioco("cambio-di-lettera").Should().Be("gioco_cambio_di_lettera");
    }

    [Fact]
    public void Gli_schemi_derivati_dalle_risorse_incorporate_si_compilano()
    {
        var catalogo = new CaricatoreDescrittori().CaricaRisorse();

        var schema = DerivatoreSchemaPagina.Deriva(catalogo.PerEstrazione());

        Validatore.Valida("{}", schema).Should().NotBeEmpty();
    }
}
