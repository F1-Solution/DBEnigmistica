using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Catalogo;
using DBEnigmistica.Tipologie.Modello;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Tipologie.Catalogo;

public class FormattatoreCatalogoTests
{
    private const string AttesoEsempi =
        """
        ### esempio-anagramma (versione 1): Anagramma di esempio
        Famiglia: gioco-breve. Resa: esposto_diagramma.
        Come si riconosce: Breve testo in versi o in prosa con un diagramma tra parentesi, per esempio (5 = 5): la soluzione usa le stesse lettere della parola nascosta.
        Regole: Esempio inventato per i test. La seconda parola contiene esattamente le lettere della prima, in un altro ordine.
        Campi:
        - `esposto` (Esposto): testo, al massimo 1000 caratteri, obbligatorio
        - `diagramma` (Diagramma): testo, al massimo 100 caratteri. Il diagramma tra parentesi, senza parentesi.
        - `soluzione` (Soluzione): testo, al massimo 100 caratteri. Solo se stampata sulla pagina.
        - `autore` (Autore): testo, al massimo 100 caratteri

        ### esempio-cruciverba (versione 1): Cruciverba di esempio
        Famiglia: cruciverba. Resa: griglia.
        Come si riconosce: Griglia rettangolare di caselle bianche e nere con i numeri delle definizioni, seguita dalle definizioni orizzontali e verticali.
        Regole: Esempio inventato per i test. Ogni parola occupa le caselle bianche consecutive di una riga o di una colonna. Le caselle nere separano le parole.
        Campi:
        - `righe` (Righe): numero intero, obbligatorio
        - `colonne` (Colonne): numero intero, obbligatorio
        - `griglia` (Griglia): griglia, nel formato descritto sopra, obbligatorio
        - `autore` (Autore): testo, al massimo 100 caratteri. Firma dell'autore, se stampata.
        Elenco `definizioni` (Definizioni), una voce per riga stampata:
        - `numero` (numero): numero intero, obbligatorio
        - `direzione` (direzione): uno tra: orizzontale, verticale, obbligatorio
        - `testo` (testo): testo, al massimo 300 caratteri, obbligatorio
        - `soluzione` (soluzione): testo, al massimo 40 caratteri. Solo se stampata sulla pagina.
        """;

    [Fact]
    public void Formatta_i_due_esempi_secondo_la_sezione_5_2()
    {
        var testo = FormattatoreCatalogo.Formatta(PercorsiTipologie.DueEsempi());

        testo.Should().Be(AttesoEsempi.Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    [Fact]
    public void L_ordine_dell_input_non_cambia_il_testo()
    {
        var invertiti = PercorsiTipologie.DueEsempi().Reverse();

        FormattatoreCatalogo.Formatta(invertiti).Should().Be(FormattatoreCatalogo.Formatta(PercorsiTipologie.DueEsempi()));
    }

    [Fact]
    public void Il_testo_non_ha_a_capo_finale_ne_CR()
    {
        var testo = FormattatoreCatalogo.Formatta(PercorsiTipologie.DueEsempi());

        testo.Should().NotEndWith("\n").And.NotContain("\r");
    }

    [Fact]
    public void Descrive_ogni_tipo_di_campo()
    {
        var campi = new[]
        {
            new CampoDescrittore("a", TipoCampo.TestoLungo), new CampoDescrittore("b", TipoCampo.Decimale),
            new CampoDescrittore("c", TipoCampo.Booleano), new CampoDescrittore("d", TipoCampo.Data),
            new CampoDescrittore("e", TipoCampo.Immagine, "Immagine"),
        };
        var d = new Descrittore("t", 1, FamigliaTipologia.Rebus, "T", "d", new RegoleTipologia("r", 1, StatoRegole.Bozza), campi, [],
            new ResaDescrittore(ModelloResa.ImmagineDiagramma), []);

        var testo = FormattatoreCatalogo.Formatta([d]);

        testo.Should().Contain("- `a` (a): testo lungo\n- `b` (b): numero decimale\n- `c` (c): vero o falso\n- `d` (d): data AAAA-MM-GG\n- `e` (Immagine): riquadro dell'immagine");
    }

    [Fact]
    public void Rifiuta_prototipi_e_id_ripetuti()
    {
        var v0 = new CaricatoreDescrittori().CaricaCartella(PercorsiTipologie.Cartella).Descrittori.First(x => x.EPrototipo);
        var v1 = PercorsiTipologie.DescrittoreEsempio("valido-esempio-anagramma.v1.json");

        var conPrototipo = () => FormattatoreCatalogo.Formatta([v0]);
        var conDoppio = () => FormattatoreCatalogo.Formatta([v1, v1]);

        conPrototipo.Should().Throw<ArgumentException>().WithMessage("*prototipo*");
        conDoppio.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Il_catalogo_reale_di_estrazione_ha_le_quattro_Tipologie_in_ordine_di_id()
    {
        var catalogo = new CaricatoreDescrittori().CaricaCartella(PercorsiTipologie.Cartella);

        var testo = FormattatoreCatalogo.Formatta(catalogo.PerEstrazione());

        var intestazioni = testo.Split('\n').Where(r => r.StartsWith("### ", StringComparison.Ordinal)).ToList();
        intestazioni.Should().HaveCount(4);
        intestazioni.Should().BeInAscendingOrder(StringComparer.Ordinal);
    }
}
