using DBEnigmistica.Console.Cli;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Generatore;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Tipologie.Generatore;

/// <summary><c>dbe types generate [--check]</c>: scrittura, controllo, file mancanti, alterati, orfani e versioni 0.</summary>
public sealed class AllineamentoSqlTests : IDisposable
{
    private readonly string _radice = Path.Combine(Path.GetTempPath(), "types-" + Guid.NewGuid().ToString("N"));

    public AllineamentoSqlTests()
    {
        var cartella = Directory.CreateDirectory(Path.Combine(_radice, "tipologie")).FullName;
        Directory.CreateDirectory(Path.Combine(cartella, "_meta"));
        Copia("anagramma", "v1.json");
        Copia("sudoku", "v0.json");
        Cartella = cartella;
    }

    private string Cartella { get; }

    public void Dispose() => Directory.Delete(_radice, true);

    private void Copia(string id, string file)
    {
        var destinazione = Directory.CreateDirectory(Path.Combine(_radice, "tipologie", id)).FullName;
        File.Copy(Path.Combine(PercorsiTipologie.Cartella, id, file), Path.Combine(destinazione, file));
    }

    private static CatalogoTipologie Catalogo(string cartella) => new CaricatoreDescrittori().CaricaCartella(cartella);

    private async Task<(int Codice, string Out, string Err)> EseguiAsync(params string[] argomenti)
    {
        var o = new StringWriter();
        var e = new StringWriter();
        var codice = await CliApp.RunAsync(["types", "generate", .. argomenti, "--cartella", Cartella], o, e);
        return (codice, o.ToString(), e.ToString());
    }

    [Fact]
    public void Un_descrittore_v1_senza_sql_fa_fallire_il_controllo_e_non_scrive()
    {
        var esito = AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: false);

        esito.EAllineato.Should().BeFalse();
        esito.Mancanti.Should().BeEquivalentTo(["anagramma/v1.riviste.sql", "anagramma/v1.catalogo.sql"]);
        Directory.EnumerateFiles(Cartella, "*.sql", SearchOption.AllDirectories).Should().BeEmpty();
    }

    [Fact]
    public void La_generazione_scrive_i_file_e_poi_il_controllo_passa_saltando_la_v0()
    {
        var scritti = AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: true);
        var controllo = AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: false);

        scritti.Scritti.Should().HaveCount(2);
        controllo.EAllineato.Should().BeTrue();
        controllo.Invariati.Should().Be(2);
        Directory.EnumerateFiles(Path.Combine(Cartella, "sudoku"), "*.sql").Should().BeEmpty();
        File.ReadAllBytes(Path.Combine(Cartella, "anagramma", "v1.riviste.sql")).Take(3).Should().NotEqual(new byte[] { 0xEF, 0xBB, 0xBF });
    }

    [Fact]
    public void Un_file_alterato_e_uno_cancellato_sono_segnalati_e_la_generazione_li_ripristina()
    {
        AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: true);
        File.AppendAllText(Path.Combine(Cartella, "anagramma", "v1.riviste.sql"), "DROP TABLE riviste.games;\n");
        File.Delete(Path.Combine(Cartella, "anagramma", "v1.catalogo.sql"));

        var controllo = AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: false);
        var ripristino = AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: true);

        controllo.Differenti.Should().Equal("anagramma/v1.riviste.sql");
        controllo.Mancanti.Should().Equal("anagramma/v1.catalogo.sql");
        ripristino.Scritti.Should().HaveCount(2);
        AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: false).EAllineato.Should().BeTrue();
    }

    [Fact]
    public void Un_file_con_a_capo_CRLF_coincide_ancora()
    {
        AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: true);
        foreach (var file in Directory.EnumerateFiles(Cartella, "*.sql", SearchOption.AllDirectories))
            File.WriteAllText(file, File.ReadAllText(file).Replace("\n", "\r\n", StringComparison.Ordinal));

        AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: false).EAllineato.Should().BeTrue();
    }

    [Fact]
    public void Un_sql_per_una_v0_o_senza_descrittore_e_orfano_e_non_si_cancella()
    {
        AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: true);
        File.WriteAllText(Path.Combine(Cartella, "sudoku", "v0.riviste.sql"), "-- non dovrebbe esistere\n");
        File.WriteAllText(Path.Combine(Cartella, "anagramma", "v2.riviste.sql"), "-- non dovrebbe esistere\n");

        var esito = AllineamentoSql.Allinea(Catalogo(Cartella).Descrittori, Cartella, scrivi: true);

        esito.Orfani.Should().BeEquivalentTo(["anagramma/v2.riviste.sql", "sudoku/v0.riviste.sql"]);
        esito.EAllineato.Should().BeFalse();
        File.Exists(Path.Combine(Cartella, "sudoku", "v0.riviste.sql")).Should().BeTrue();
    }

    [Fact]
    public async Task Il_comando_con_check_esce_con_un_codice_diverso_da_0_se_manca_l_sql()
    {
        var (codice, _, errori) = await EseguiAsync("--check");

        codice.Should().NotBe(0);
        errori.Should().Contain("SQL mancante: anagramma/v1.riviste.sql");
    }

    [Fact]
    public async Task Il_comando_genera_poi_il_check_passa_e_la_v0_non_produce_nulla()
    {
        var (codiceGenera, uscitaGenera, _) = await EseguiAsync();
        var (codiceCheck, uscitaCheck, _) = await EseguiAsync("--check");

        codiceGenera.Should().Be(0);
        uscitaGenera.Should().Contain("Scritto anagramma/v1.riviste.sql");
        codiceCheck.Should().Be(0);
        uscitaCheck.Should().Contain("allineato: 2 file");
        Directory.EnumerateFiles(Path.Combine(Cartella, "sudoku")).Select(Path.GetFileName).Should().Equal("v0.json");
    }

    [Fact]
    public async Task Un_descrittore_non_valido_da_un_errore_con_i_messaggi()
    {
        File.WriteAllText(Path.Combine(Cartella, "anagramma", "v1.json"), "{ \"id\": \"anagramma\" }");

        var (codice, _, errori) = await EseguiAsync("--check");

        codice.Should().Be(1);
        errori.Should().Contain("Descrittore non valido");
    }

    [Fact]
    public void L_sql_registrato_nel_repository_coincide_con_quello_generato()
    {
        var catalogo = Catalogo(PercorsiTipologie.Cartella);

        var esito = AllineamentoSql.Allinea(catalogo.Descrittori, PercorsiTipologie.Cartella, scrivi: false);

        esito.EAllineato.Should().BeTrue($"differenti: {string.Join(", ", esito.Differenti)}; mancanti: {string.Join(", ", esito.Mancanti)}; orfani: {string.Join(", ", esito.Orfani)}");
    }
}
