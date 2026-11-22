using System.Text.RegularExpressions;
using DBEnigmistica.Tipologie.Generatore;
using DBEnigmistica.Tipologie.Modello;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Tipologie.Generatore;

/// <summary>Fuzz con 10.000 descrittori e seme fisso: ogni descrittore è rifiutato oppure produce solo righe della lista ammessa.</summary>
public partial class GeneratoreSqlFuzzTests
{
    private const string Identificatore = "[a-z][a-z0-9_]*";
    private const string Tipo = @"(integer|uuid|text|jsonb|boolean|timestamptz|numeric\(12,4\)|varchar\([0-9]{1,4}\))";

    private static readonly Regex[] RigheAmmesse =
    [
        IntestazioneRegex(),
        new($"^CREATE TABLE (riviste|catalogo)\\.\"{Identificatore}\" \\($"),
        new($"^    \"{Identificatore}\" {Tipo}( NOT NULL)?,$"),
        new($"^    CONSTRAINT \"{Identificatore}\" PRIMARY KEY \\(\"gioco_id\"(, \"posizione\")?\\),$"),
        new($"^    CONSTRAINT \"{Identificatore}\" FOREIGN KEY \\(\"gioco_id\"\\) REFERENCES (riviste\\.\"games\" \\(\"Id\"\\)|catalogo\\.\"giochi\" \\(\"id\"\\)|(riviste|catalogo)\\.\"{Identificatore}\" \\(\"gioco_id\"\\)) ON DELETE CASCADE,?$"),
        new($"^    CONSTRAINT \"{Identificatore}\" CHECK \\(\"posizione\" >= 0\\),?$"),
        new($"^    CONSTRAINT \"{Identificatore}\" CHECK \\(\"{Identificatore}\" IN \\('[a-z0-9_]{{1,40}}'(, '[a-z0-9_]{{1,40}}')*\\)\\),?$"),
        new(@"^\);$"),
        new($"^CREATE INDEX \"{Identificatore}\" ON (riviste|catalogo)\\.\"{Identificatore}\" \\(\"{Identificatore}\"\\);$"),
        new("^$"),
    ];

    [GeneratedRegex("^-- Generato da dbe types generate: [a-z0-9-]+ v[0-9]{1,3}, schema (riviste|catalogo)\\. Non modificare a mano\\.$")]
    private static partial Regex IntestazioneRegex();

    [Fact]
    public void Ogni_descrittore_casuale_e_rifiutato_o_produce_solo_righe_ammesse()
    {
        int accettati = 0, rifiutati = 0;
        foreach (var d in FuzzDescrittori.Genera())
        {
            foreach (var schema in new[] { SchemaTipologia.Riviste, SchemaTipologia.Catalogo })
            {
                FileSql file;
                try
                {
                    file = GeneratoreSql.Genera(d, schema);
                }
                catch (DescrittoreNonGenerabileException)
                {
                    rifiutati++;
                    continue;
                }

                accettati++;
                file.Contenuto.Should().EndWith(";\n");
                foreach (var riga in file.Contenuto.TrimEnd('\n').Split('\n'))
                    Assert.True(RigheAmmesse.Any(re => re.IsMatch(riga)), $"riga non ammessa: {riga}");
            }
        }

        accettati.Should().BeGreaterThan(1000, "il fuzz deve produrre anche descrittori validi");
        rifiutati.Should().BeGreaterThan(1000, "il fuzz deve produrre anche descrittori ostili");
    }

    [Fact]
    public void La_sequenza_del_fuzz_e_riproducibile()
    {
        var uno = FuzzDescrittori.Genera(200).Select(d => d.Id + d.Campi.Count).ToList();
        var due = FuzzDescrittori.Genera(200).Select(d => d.Id + d.Campi.Count).ToList();

        due.Should().Equal(uno);
    }
}
