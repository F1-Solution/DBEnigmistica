namespace DBEnigmistica.Domain.Entities;

/// <summary>Una pagina da cancellare è pagina-soluzione di un Gioco che resta.</summary>
public sealed class PaginaSoluzioneInUsoException(string message) : InvalidOperationException(message);
