namespace DBEnigmistica.Console.Cli;

/// <summary>Codici d'uscita dell'eseguibile.</summary>
public static class ExitCodes
{
    public const int Success = 0;
    public const int Error = 1;
    public const int NotAvailable = 2;
    public const int Configuration = 3;
    /// <summary>Limite dell'esecuzione o del fornitore raggiunto.</summary>
    public const int LimitReached = 4;
    /// <summary>Catena delle impronte del registro alterata.</summary>
    public const int RegistryTampered = 5;
    public const int Cancelled = 130;
}
