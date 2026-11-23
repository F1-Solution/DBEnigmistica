using System.Text.RegularExpressions;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Generatore;
using DBEnigmistica.Tipologie.Modello;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Tipologie.Generatore;

public partial class GeneratoreSqlTests
{
    private const string AttesoRiviste =
        """
        -- Generato da dbe types generate: esempio-cruciverba v1, schema riviste. Non modificare a mano.

        CREATE TABLE riviste."gioco_esempio_cruciverba_v1" (
            "gioco_id" integer NOT NULL,
            "righe" integer NOT NULL,
            "colonne" integer NOT NULL,
            "griglia" jsonb NOT NULL,
            "autore" varchar(100),
            CONSTRAINT "pk_gioco_esempio_cruciverba_v1" PRIMARY KEY ("gioco_id"),
            CONSTRAINT "fk_gioco_esempio_cruciverba_v1__gioco" FOREIGN KEY ("gioco_id") REFERENCES riviste."games" ("Id") ON DELETE CASCADE
        );

        CREATE TABLE riviste."gioco_esempio_cruciverba_v1__definizioni" (
            "gioco_id" integer NOT NULL,
            "posizione" integer NOT NULL,
            "numero" integer NOT NULL,
            "direzione" text NOT NULL,
            "testo" varchar(300) NOT NULL,
            "soluzione" varchar(40),
            CONSTRAINT "pk_gioco_esempio_cruciverba_v1__definizioni" PRIMARY KEY ("gioco_id", "posizione"),
            CONSTRAINT "fk_gioco_esempio_cruciverba_v1__definizioni__gioco" FOREIGN KEY ("gioco_id") REFERENCES riviste."gioco_esempio_cruciverba_v1" ("gioco_id") ON DELETE CASCADE,
            CONSTRAINT "ck_gioco_esempio_cruciverba_v1__definizioni__posizione" CHECK ("posizione" >= 0),
            CONSTRAINT "ck_gioco_esempio_cruciverba_v1__definizioni__direzione" CHECK ("direzione" IN ('orizzontale', 'verticale'))
        );

        CREATE INDEX "ix_gioco_esempio_cruciverba_v1__autore" ON riviste."gioco_esempio_cruciverba_v1" ("autore");

        """;

    private static Descrittore Cruciverba() => PercorsiTipologie.DescrittoreEsempio("valido-esempio-cruciverba.v1.json");

    private static string Lf(string testo) => testo.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static Descrittore Semplice(string id = "prova", IReadOnlyList<CampoDescrittore>? campi = null, IReadOnlyList<ElencoDescrittore>? elenchi = null) =>
        new(id, 1, FamigliaTipologia.GiocoBreve, "Prova", "d", new RegoleTipologia("r", 1, StatoRegole.Bozza),
            campi ?? [new CampoDescrittore("esposto", TipoCampo.Testo, Max: 100, Obbligatorio: true)], elenchi ?? [],
            new ResaDescrittore(ModelloResa.EspostoDiagramma), []);

    [Fact]
    public void Due_versioni_della_stessa_Tipologia_generano_tabelle_e_vincoli_distinti_senza_conflitti()
    {
        var elenchi = new[] { new ElencoDescrittore("voci", [new CampoDescrittore("b", TipoCampo.Intero, Indice: true)]) };
        var file = GeneratoreSql.GeneraTutti([
            Semplice("prova", elenchi: elenchi) with { Versione = 1 },
            Semplice("prova", elenchi: elenchi) with { Versione = 2 }]);

        file.Select(f => f.Percorso).Should().Equal(
            "prova/v1.riviste.sql", "prova/v1.catalogo.sql", "prova/v2.riviste.sql", "prova/v2.catalogo.sql");
        foreach (var schema in new[] { SchemaTipologia.Riviste, SchemaTipologia.Catalogo })
        {
            var nomi = file.Where(f => f.Schema == schema)
                .SelectMany(f => CreateNomi().Matches(f.Contenuto).Select(m => m.Groups[1].Value)).ToList();
            nomi.Should().Contain(["gioco_prova_v1", "gioco_prova_v1__voci", "gioco_prova_v2", "gioco_prova_v2__voci"]);
            nomi.Should().OnlyHaveUniqueItems();
        }

        file[2].Contenuto.Should().Contain("CONSTRAINT \"pk_gioco_prova_v2\"").And.Contain("\"ix_gioco_prova_v2__voci__b\"").And.NotContain("_v1");
    }

    [Fact]
    public void Ai_limiti_del_meta_schema_con_la_versione_i_nomi_di_tabella_arrivano_a_63_byte_senza_superarli()
    {
        var d = Semplice(new string('a', 30), elenchi: [new ElencoDescrittore(new string('b', 20), [new CampoDescrittore("c", TipoCampo.Intero)])]) with { Versione = 999 };

        var sql = GeneratoreSql.Genera(d, SchemaTipologia.Riviste).Contenuto;

        var tabelle = CreateNomi().Matches(sql).Select(m => m.Groups[1].Value).ToList();
        tabelle.Should().HaveCount(2).And.OnlyContain(n => n.Length <= 63);
        tabelle.Should().Contain(n => n.Length == 63 && n.EndsWith("_v999__" + new string('b', 20), StringComparison.Ordinal));
    }

    [GeneratedRegex("CREATE TABLE \\w+\\.\"([a-z0-9_]+)\"")]
    private static partial Regex CreateNomi();

    [Fact]
    public void Lo_snapshot_dell_esempio_della_sezione_3_1_coincide_byte_per_byte()
    {
        var file = GeneratoreSql.Genera(Cruciverba(), SchemaTipologia.Riviste);

        file.Contenuto.Should().Be(Lf(AttesoRiviste));
        file.Percorso.Should().Be("esempio-cruciverba/v1.riviste.sql");
    }

    [Fact]
    public void Lo_schema_catalogo_usa_uuid_e_il_nucleo_del_catalogo()
    {
        var sql = GeneratoreSql.Genera(Cruciverba(), SchemaTipologia.Catalogo).Contenuto;

        sql.Should().StartWith("-- Generato da dbe types generate: esempio-cruciverba v1, schema catalogo. Non modificare a mano.\n\nCREATE TABLE catalogo.\"gioco_esempio_cruciverba_v1\" (\n    \"gioco_id\" uuid NOT NULL,");
        sql.Should().Contain("REFERENCES catalogo.\"giochi\" (\"id\") ON DELETE CASCADE");
        sql.Should().Contain("REFERENCES catalogo.\"gioco_esempio_cruciverba_v1\" (\"gioco_id\")");
        sql.Should().NotContain("riviste.");
        sql.Should().EndWith(";\n");
    }

    [Fact]
    public void L_output_e_deterministico_con_a_capo_LF_senza_BOM()
    {
        var uno = GeneratoreSql.Genera(Cruciverba(), SchemaTipologia.Riviste).Contenuto;
        var due = GeneratoreSql.Genera(Cruciverba(), SchemaTipologia.Riviste).Contenuto;

        due.Should().Be(uno);
        uno.Should().NotContain("\r").And.NotStartWith("﻿");
    }

    [Fact]
    public void Il_prototipo_non_ha_sql_e_GeneraTutti_lo_salta()
    {
        var catalogo = new CaricatoreDescrittori().CaricaCartella(PercorsiTipologie.Cartella);
        var v0 = catalogo.Descrittori.First(d => d.EPrototipo);

        var azione = () => GeneratoreSql.Genera(v0, SchemaTipologia.Riviste);
        var tutti = GeneratoreSql.GeneraTutti(catalogo.Descrittori);

        azione.Should().Throw<DescrittoreNonGenerabileException>().WithMessage("*prototipo*");
        tutti.Should().NotContain(f => f.IdTipologia == "sudoku");
        tutti.Select(f => f.Percorso).Should().Contain(["anagramma/v1.riviste.sql", "anagramma/v1.catalogo.sql", "cruciverba-classico/v1.riviste.sql"]);
        tutti.Should().HaveCount(catalogo.ConSql().Count * 2);
    }

    [Fact]
    public void Le_Tipologie_reali_non_contengono_istruzioni_vietate()
    {
        var catalogo = new CaricatoreDescrittori().CaricaCartella(PercorsiTipologie.Cartella);

        foreach (var file in GeneratoreSql.GeneraTutti(catalogo.Descrittori))
        {
            var istruzioni = file.Contenuto.Split('\n').Where(r => !r.StartsWith("--", StringComparison.Ordinal)).ToList();
            var testo = string.Join('\n', istruzioni);
            testo.Should().NotMatchRegex(@"\b(DEFAULT|TRIGGER|FUNCTION|GRANT|ALTER|DROP|BEGIN|COMMIT|SET|SEQUENCE|IDENTITY)\b", file.Percorso);
        }
    }

    [Theory]
    [InlineData("a\"; DROP TABLE riviste.games;--")]
    [InlineData("Maiuscolo")]
    [InlineData("con spazio")]
    [InlineData("a\n")]
    [InlineData("")]
    [InlineData("1iniziale")]
    [InlineData("select")]
    [InlineData("gioco_id")]
    [InlineData("posizione")]
    [InlineData("àccento")]
    public void Un_nome_di_campo_ostile_o_riservato_e_rifiutato(string nome)
    {
        var d = Semplice(campi: [new CampoDescrittore(nome, TipoCampo.Intero)]);

        var azione = () => GeneratoreSql.Genera(d, SchemaTipologia.Riviste);

        azione.Should().Throw<DescrittoreNonGenerabileException>();
    }

    [Theory]
    [InlineData("x'); DROP TABLE riviste.games;--")]
    [InlineData("Maiuscolo")]
    [InlineData("a\n")]
    [InlineData("")]
    public void Un_valore_di_enumerazione_ostile_e_rifiutato(string valore)
    {
        var d = Semplice(campi: [new CampoDescrittore("scelta", TipoCampo.Enumerazione, Valori: [valore])]);

        var azione = () => GeneratoreSql.Genera(d, SchemaTipologia.Riviste);

        azione.Should().Throw<DescrittoreNonGenerabileException>();
    }

    [Theory]
    [InlineData("a\"; DROP TABLE x;--")]
    [InlineData("a\n")]
    [InlineData("Maiuscolo")]
    [InlineData("-iniziale")]
    [InlineData("doppio--trattino")]
    [InlineData("troppo-lungo-troppo-lungo-troppo-lungo")]
    public void Un_id_di_Tipologia_ostile_e_rifiutato(string id)
    {
        var azione = () => GeneratoreSql.Genera(Semplice(id), SchemaTipologia.Riviste);

        azione.Should().Throw<DescrittoreNonGenerabileException>();
    }

    [Fact]
    public void Tipi_fuori_lista_e_parametri_incoerenti_sono_rifiutati()
    {
        CampoDescrittore[][] casi =
        [
            [new CampoDescrittore("a", (TipoCampo)99)],
            [new CampoDescrittore("a", TipoCampo.Testo)],
            [new CampoDescrittore("a", TipoCampo.Testo, Max: 0)],
            [new CampoDescrittore("a", TipoCampo.Testo, Max: 4001)],
            [new CampoDescrittore("a", TipoCampo.Intero, Max: 10)],
            [new CampoDescrittore("a", TipoCampo.Intero, Valori: ["x"])],
            [new CampoDescrittore("a", TipoCampo.Enumerazione)],
            [new CampoDescrittore("a", TipoCampo.Griglia, Indice: true)],
            [new CampoDescrittore("a", TipoCampo.Intero), new CampoDescrittore("a", TipoCampo.Intero)],
        ];

        foreach (var campi in casi)
        {
            var azione = () => GeneratoreSql.Genera(Semplice(campi: campi), SchemaTipologia.Riviste);
            azione.Should().Throw<DescrittoreNonGenerabileException>();
        }
    }

    [Fact]
    public void Una_griglia_in_un_elenco_e_un_nome_di_elenco_oltre_20_caratteri_sono_rifiutati()
    {
        var conGriglia = Semplice(elenchi: [new ElencoDescrittore("voci", [new CampoDescrittore("g", TipoCampo.Griglia)])]);
        var elencoLungo = Semplice(elenchi: [new ElencoDescrittore(new string('e', 21), [new CampoDescrittore("n", TipoCampo.Intero)])]);

        var a = () => GeneratoreSql.Genera(conGriglia, SchemaTipologia.Riviste);
        var b = () => GeneratoreSql.Genera(elencoLungo, SchemaTipologia.Riviste);

        a.Should().Throw<DescrittoreNonGenerabileException>();
        b.Should().Throw<DescrittoreNonGenerabileException>();
    }

    [Fact]
    public void Un_nome_di_vincolo_oltre_63_byte_si_accorcia_con_l_impronta()
    {
        var id = new string('a', 30);
        var d = Semplice(id, elenchi: [new ElencoDescrittore(new string('b', 20), [new CampoDescrittore(new string('c', 41), TipoCampo.Enumerazione, Valori: ["x"])])]);

        var sql = GeneratoreSql.Genera(d, SchemaTipologia.Riviste).Contenuto;

        var nomi = NomiQuotati().Matches(sql).Select(m => m.Groups[1].Value).Distinct().ToList();
        nomi.Should().OnlyContain(n => n.Length <= 63);
        nomi.Should().Contain(n => n.Length == 63 && n.StartsWith("ck_", StringComparison.Ordinal) && Regex.IsMatch(n, "_[0-9a-f]{8}$"));
        nomi.Count.Should().Be(nomi.Distinct().Count());
    }

    [Fact]
    public void L_indice_vale_per_i_campi_dell_elenco_e_segue_le_tabelle()
    {
        var d = Semplice(
            campi: [new CampoDescrittore("a", TipoCampo.Intero, Indice: true)],
            elenchi: [new ElencoDescrittore("voci", [new CampoDescrittore("b", TipoCampo.Data, Indice: true)])]);

        var sql = GeneratoreSql.Genera(d, SchemaTipologia.Riviste).Contenuto;

        sql.IndexOf("CREATE INDEX", StringComparison.Ordinal).Should().BeGreaterThan(sql.LastIndexOf("CREATE TABLE", StringComparison.Ordinal));
        sql.Should().Contain("CREATE INDEX \"ix_gioco_prova_v1__a\" ON riviste.\"gioco_prova_v1\" (\"a\");")
            .And.Contain("CREATE INDEX \"ix_gioco_prova_v1__voci__b\" ON riviste.\"gioco_prova_v1__voci\" (\"b\");")
            .And.Contain("\"b\" timestamptz");
    }

    [GeneratedRegex("\"((?:pk|fk|ck|ix)_[a-z0-9_]+)\"")]
    private static partial Regex NomiQuotati();
}
