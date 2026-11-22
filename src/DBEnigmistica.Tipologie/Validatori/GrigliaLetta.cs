using System.Text.Json;

namespace DBEnigmistica.Tipologie.Validatori;

internal sealed record NumeroInGriglia(int Riga, int Colonna, int Numero);

/// <summary>Parola della griglia: <c>Direzione</c> è "orizzontale" o "verticale"; riga e colonna partono da 1.</summary>
internal sealed record ParolaInGriglia(int Numero, string Direzione, int Riga, int Colonna, int Lunghezza)
{
    public const string Orizzontale = "orizzontale";
    public const string Verticale = "verticale";
}

/// <summary>Valore di un campo griglia: una stringa per riga, '#' casella nera, il resto casella bianca.</summary>
internal sealed class GrigliaLetta
{
    private GrigliaLetta(int righe, int colonne, IReadOnlyList<string> celle, IReadOnlyList<NumeroInGriglia> numeri)
    {
        Righe = righe;
        Colonne = colonne;
        Celle = celle;
        Numeri = numeri;
    }

    public int Righe { get; }

    public int Colonne { get; }

    public IReadOnlyList<string> Celle { get; }

    public IReadOnlyList<NumeroInGriglia> Numeri { get; }

    /// <summary>Casella bianca alla posizione (da 0); fuori dalla griglia è come una casella nera.</summary>
    public bool Bianca(int riga, int colonna) =>
        riga >= 0 && riga < Celle.Count && colonna >= 0 && colonna < Celle[riga].Length && Celle[riga][colonna] != '#';

    /// <summary>Legge la griglia; con <paramref name="verificaForma"/> pretende che sia rettangolare e uguale alle dimensioni dichiarate.</summary>
    public static bool TryLeggi(JsonElement valore, bool verificaForma, out GrigliaLetta? griglia, out string errore)
    {
        griglia = null;
        if (valore.ValueKind != JsonValueKind.Object)
            return Errore("La griglia deve essere un oggetto con righe, colonne, celle e numeri.", out errore);
        var righe = LetturaDati.Intero(valore, "righe");
        var colonne = LetturaDati.Intero(valore, "colonne");
        if (righe is null || colonne is null)
            return Errore("La griglia deve avere 'righe' e 'colonne' interi.", out errore);
        if (LetturaDati.Elemento(valore, "celle") is not { ValueKind: JsonValueKind.Array } celleJson || celleJson.EnumerateArray().Any(c => c.ValueKind != JsonValueKind.String))
            return Errore("La griglia deve avere 'celle', un elenco di stringhe.", out errore);
        var celle = celleJson.EnumerateArray().Select(c => c.GetString()!).ToList();
        if (!TryNumeri(valore, out var numeri))
            return Errore("I 'numeri' della griglia devono avere riga, colonna e numero interi.", out errore);

        if (verificaForma && (celle.Count != righe || celle.Any(c => c.Length != colonne)))
            return Errore("La griglia non è rettangolare o non ha le dimensioni dichiarate.", out errore);

        griglia = new GrigliaLetta(righe.Value, colonne.Value, celle, numeri);
        errore = string.Empty;
        return true;
    }

    private static bool TryNumeri(JsonElement valore, out List<NumeroInGriglia> numeri)
    {
        numeri = [];
        if (LetturaDati.Elemento(valore, "numeri") is not { } elenco)
            return true;
        if (elenco.ValueKind != JsonValueKind.Array)
            return false;
        foreach (var voce in elenco.EnumerateArray())
        {
            if (LetturaDati.Intero(voce, "riga") is not { } r || LetturaDati.Intero(voce, "colonna") is not { } c || LetturaDati.Intero(voce, "numero") is not { } n)
                return false;
            numeri.Add(new NumeroInGriglia(r, c, n));
        }

        return true;
    }

    private static bool Errore(string messaggio, out string errore)
    {
        errore = messaggio;
        return false;
    }
}
