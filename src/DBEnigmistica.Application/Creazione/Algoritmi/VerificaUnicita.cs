namespace DBEnigmistica.Application.Creazione.Algoritmi;

/// <summary>Esito del conteggio delle soluzioni, fermato a 2.</summary>
public enum EsitoUnicita
{
    Nessuna = 0,
    Unica = 1,
    Multiple = 2,
}

/// <summary>Verifica della soluzione unica, da richiamare dopo ogni modifica del revisore.</summary>
public static class VerificaUnicita
{
    public static EsitoUnicita Sudoku(IReadOnlyList<int> indizi) =>
        Da(SudokuRisolutore.ContaSoluzioni(indizi, 2));

    public static EsitoUnicita Futoshiki(int dimensione, IReadOnlyList<int> indizi, IReadOnlyList<SegnoFutoshiki> segni) =>
        Da(FutoshikiRisolutore.ContaSoluzioni(dimensione, indizi, segni, 2));

    public static EsitoUnicita Nonogramma(
        int righe,
        int colonne,
        IReadOnlyList<IReadOnlyList<int>> indiziRighe,
        IReadOnlyList<IReadOnlyList<int>> indiziColonne) =>
        Da(NonogrammaRisolutore.ContaSoluzioni(righe, colonne, indiziRighe, indiziColonne, 2));

    private static EsitoUnicita Da(int soluzioni) => soluzioni switch
    {
        0 => EsitoUnicita.Nessuna,
        1 => EsitoUnicita.Unica,
        _ => EsitoUnicita.Multiple,
    };
}
