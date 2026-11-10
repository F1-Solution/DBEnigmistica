namespace DBEnigmistica.Console.Cli;

/// <summary>Risorse che un comando richiede all'avvio e che vengono validate prima dell'esecuzione.</summary>
[Flags]
public enum HostRequirements
{
    None = 0,
    Database = 1,
    Ai = 2
}
