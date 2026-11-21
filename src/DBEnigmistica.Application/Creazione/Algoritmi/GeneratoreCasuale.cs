namespace DBEnigmistica.Application.Creazione.Algoritmi;

/// <summary>Generatore pseudocasuale SplitMix64: stesso seme, stessa sequenza su ogni versione di .NET.</summary>
public sealed class GeneratoreCasuale
{
    private ulong _stato;

    public GeneratoreCasuale(long seme) => _stato = unchecked((ulong)seme);

    public ulong ProssimoU64()
    {
        unchecked
        {
            var z = _stato += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    /// <summary>Intero in [0, massimo).</summary>
    public int Prossimo(int massimo)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(massimo, 0);
        return (int)(((ProssimoU64() >> 32) * (ulong)massimo) >> 32);
    }

    /// <summary>Numero in [0, 1).</summary>
    public double ProssimoDouble() => (ProssimoU64() >> 11) * (1.0 / (1UL << 53));

    /// <summary>Mescola sul posto con Fisher-Yates.</summary>
    public void Mescola<T>(Span<T> elementi)
    {
        for (var i = elementi.Length - 1; i > 0; i--)
        {
            var j = Prossimo(i + 1);
            (elementi[i], elementi[j]) = (elementi[j], elementi[i]);
        }
    }
}
