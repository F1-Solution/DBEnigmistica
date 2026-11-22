using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tests.Unit.Tipologie.Generatore;

/// <summary>Descrittori casuali, in parte ostili, costruiti senza passare dal caricatore: il generatore deve rifiutarli o quotarli.</summary>
internal static class FuzzDescrittori
{
    /// <summary>Seme registrato: stessa sequenza a ogni esecuzione.</summary>
    public const int Seme = 20261121;

    public const int Quanti = 10_000;

    private static readonly string[] Ostili =
    [
        "a\"; DROP TABLE riviste.games;--", "x'); DELETE FROM registro.eventi;--", "Maiuscolo", "con spazio", "a\n", "a\r\nb", "",
        "select", "table", "user", "id", "gioco_id", "posizione", "oid", "xmin", "àccento", "a-b", "1abc", "a\"b", "a'b", "a;b",
        "\0", "a/*b*/", "$$x$$", "a\\b", new string('x', 80),
    ];

    private static readonly TipoCampo[] Tipi = [.. Enum.GetValues<TipoCampo>()];

    public static IEnumerable<Descrittore> Genera(int quanti = Quanti)
    {
        var caso = new Random(Seme);
        for (var i = 0; i < quanti; i++)
            yield return Uno(caso);
    }

    private static Descrittore Uno(Random r)
    {
        // Tre descrittori su dieci portano valori ostili; gli altri sono puliti ma comunque casuali.
        var ostile = r.Next(10) < 3;
        var campi = Enumerable.Range(1, r.Next(1, 8)).Select(i => Campo(r, ostile, consentiGriglia: true, i)).ToList();
        var elenchi = Enumerable.Range(0, r.Next(0, 3)).Select(e => new ElencoDescrittore(
            Nome(r, ostile, 20, "e" + e), [.. Enumerable.Range(1, r.Next(1, 5)).Select(i => Campo(r, ostile, consentiGriglia: false, i))])).ToList();
        var versione = ostile && r.Next(4) == 0 ? r.Next(-2, 1200) : r.Next(1, 4);

        return new Descrittore(
            Id(r, ostile), versione, FamigliaTipologia.GiocoBreve, "Fuzz", "d", new RegoleTipologia("r", 1, StatoRegole.Bozza),
            campi, elenchi, new ResaDescrittore(ModelloResa.EspostoDiagramma), []);
    }

    private static string Pezzo(Random r, int lunghezza, string alfabeto) =>
        string.Create(lunghezza, (r.Next(), alfabeto), static (s, st) =>
        {
            var rr = new Random(st.Item1);
            for (var i = 0; i < s.Length; i++)
                s[i] = st.alfabeto[rr.Next(st.alfabeto.Length)];
        });

    // Il suffisso rende unici i nomi dei descrittori puliti; quelli ostili possono ripetersi.
    private static string Nome(Random r, bool ostile, int massimo, string suffisso)
    {
        if (ostile && r.Next(4) == 0)
            return Ostili[r.Next(Ostili.Length)];
        var nome = Pezzo(r, r.Next(1, Math.Min(massimo - suffisso.Length - 1, 10) + 1), "abcdefghijklmnopqrstuvwxyz");
        return r.Next(3) == 0 ? nome + Pezzo(r, 1, "0123456789_") + suffisso : nome + suffisso;
    }

    private static string Id(Random r, bool ostile)
    {
        if (ostile && r.Next(3) == 0)
            return Ostili[r.Next(Ostili.Length)];
        var id = Pezzo(r, r.Next(1, 10), "abcdefghijklmnopqrstuvwxyz0123456789");
        return r.Next(3) == 0 ? id + "-" + Pezzo(r, r.Next(1, 8), "abcdefghijklmnopqrstuvwxyz0123456789") : id;
    }

    private static CampoDescrittore Campo(Random r, bool ostile, bool consentiGriglia, int numero)
    {
        var tipo = ostile && r.Next(10) == 0 ? (TipoCampo)r.Next(-1, 15) : Tipi[r.Next(Tipi.Length)];
        if (!consentiGriglia && tipo == TipoCampo.Griglia && !(ostile && r.Next(3) == 0))
            tipo = TipoCampo.Testo;

        int? max = tipo == TipoCampo.Testo ? (ostile && r.Next(5) == 0 ? r.Next(-5, 5000) : r.Next(1, 400)) : (ostile && r.Next(10) == 0 ? r.Next(1, 50) : null);
        IReadOnlyList<string>? valori = null;
        if (tipo == TipoCampo.Enumerazione || (ostile && r.Next(10) == 0))
        {
            valori = [.. Enumerable.Range(0, r.Next(ostile ? 0 : 1, 5)).Select(i =>
                ostile && r.Next(4) == 0 ? Ostili[r.Next(Ostili.Length)] : Pezzo(r, r.Next(1, 8), "abcdefghijklmnopqrstuvwxyz0123456789_") + i)];
        }

        var indice = r.Next(5) == 0 && (ostile || tipo is TipoCampo.Intero or TipoCampo.Testo or TipoCampo.Enumerazione or TipoCampo.Data);
        return new CampoDescrittore(Nome(r, ostile, 40, "_" + numero), tipo, Obbligatorio: r.Next(2) == 0, Max: max, Valori: valori, Indice: indice);
    }
}
