using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DBEnigmistica.Domain.Registro;

/// <summary>Evento da aggiungere al registro in sola aggiunta.</summary>
public sealed partial record NuovoEvento
{
    public NuovoEvento(DateTime avvenutoIl, string tipo, string entita, string entitaId, string autore, string dati)
    {
        AvvenutoIl = Controlli.Utc(avvenutoIl, nameof(avvenutoIl));
        Tipo = Nome(tipo, nameof(tipo));
        Entita = Nome(entita, nameof(entita));
        EntitaId = Controlli.Testo(entitaId, nameof(entitaId), 100);
        Autore = Controlli.Testo(autore, nameof(autore), 200);
        Dati = ControllaOggettoJson(dati);
    }

    public DateTime AvvenutoIl { get; }

    public string Tipo { get; }

    public string Entita { get; }

    public string EntitaId { get; }

    public string Autore { get; }

    public string Dati { get; }

    private static string Nome(string? valore, string nome) =>
        valore is not null && FormatoNome().IsMatch(valore)
            ? valore
            : throw new ArgumentException($"Il campo «{nome}» deve essere in minuscolo con cifre e trattini bassi.", nome);

    private static string ControllaOggettoJson(string? dati)
    {
        try
        {
            using var documento = JsonDocument.Parse(dati ?? string.Empty);
            if (documento.RootElement.ValueKind == JsonValueKind.Object)
                return dati!;
        }
        catch (JsonException)
        {
            // Il messaggio sotto spiega comunque il problema.
        }

        throw new ArgumentException("I dati dell'evento devono essere un oggetto JSON.", nameof(dati));
    }

    [GeneratedRegex("^[a-z][a-z0-9_]{0,59}$")]
    private static partial Regex FormatoNome();
}

/// <summary>Riga del registro come salvata, con le due impronte.</summary>
public sealed record EventoRegistro
{
    private EventoRegistro()
    {
    }

    public long Id { get; init; }

    public DateTime AvvenutoIl { get; init; }

    public string Tipo { get; init; } = string.Empty;

    public string Entita { get; init; } = string.Empty;

    public string EntitaId { get; init; } = string.Empty;

    public string Autore { get; init; } = string.Empty;

    public string Dati { get; init; } = string.Empty;

    public byte[] ImprontaPrecedente { get; init; } = [];

    public byte[] Impronta { get; init; } = [];

    public static EventoRegistro Da(long id, NuovoEvento e, byte[] improntaPrecedente, byte[] impronta) => new()
    {
        Id = id,
        AvvenutoIl = e.AvvenutoIl,
        Tipo = e.Tipo,
        Entita = e.Entita,
        EntitaId = e.EntitaId,
        Autore = e.Autore,
        Dati = e.Dati,
        ImprontaPrecedente = improntaPrecedente,
        Impronta = impronta
    };

    /// <summary>I campi che entrano nell'impronta, riletti dalla riga salvata.</summary>
    public NuovoEvento ComeNuovoEvento() => new(AvvenutoIl, Tipo, Entita, EntitaId, Autore, Dati);
}

/// <summary>Impronta SHA-256 concatenata: ogni riga copre l'impronta della precedente.</summary>
public static class ImprontaEvento
{
    private const int Lunghezza = 32;
    private const string Versione = "v1";
    private const string FormatoData = "yyyy-MM-ddTHH:mm:ss.ffffffZ";

    public static byte[] Iniziale => new byte[Lunghezza];

    public static byte[] Calcola(byte[] precedente, NuovoEvento evento)
    {
        if (precedente is null || precedente.Length != Lunghezza)
            throw new ArgumentException($"L'impronta precedente deve essere di {Lunghezza} byte.", nameof(precedente));

        var contenuto = Contenuto(evento);
        var dati = new byte[Lunghezza + contenuto.Length];
        precedente.CopyTo(dati, 0);
        contenuto.CopyTo(dati, Lunghezza);
        return SHA256.HashData(dati);
    }

    /// <summary>Campi in ordine, ciascuno come «lunghezza in byte UTF-8:byte,».</summary>
    public static byte[] Contenuto(NuovoEvento e)
    {
        using var flusso = new MemoryStream();
        foreach (var campo in new[]
                 {
                     Versione,
                     e.AvvenutoIl.ToString(FormatoData, System.Globalization.CultureInfo.InvariantCulture),
                     e.Tipo, e.Entita, e.EntitaId, e.Autore, e.Dati
                 })
        {
            var byteCampo = Encoding.UTF8.GetBytes(campo);
            flusso.Write(Encoding.ASCII.GetBytes($"{byteCampo.Length}:"));
            flusso.Write(byteCampo);
            flusso.WriteByte((byte)',');
        }

        return flusso.ToArray();
    }
}

public sealed record EsitoVerifica(bool Integra, long EventiVerificati, long? PrimoIdNonValido, string? Motivo);

/// <summary>Controlla che la catena delle impronte sia integra, riga per riga.</summary>
public static class VerificaCatena
{
    public const string MotivoPrecedente = "impronta precedente non coincide";
    public const string MotivoRigaNonValida = "riga non valida";
    public const string MotivoRicalcolata = "impronta ricalcolata diversa";

    public static EsitoVerifica Verifica(IEnumerable<EventoRegistro> inOrdineDiId)
    {
        var passo = new Passo();
        foreach (var riga in inOrdineDiId)
        {
            if (passo.Esamina(riga) is { } fallito)
                return fallito;
        }

        return passo.Esito();
    }

    public static async Task<EsitoVerifica> VerificaAsync(
        IAsyncEnumerable<EventoRegistro> inOrdineDiId, CancellationToken ct = default)
    {
        var passo = new Passo();
        await foreach (var riga in inOrdineDiId.WithCancellation(ct))
        {
            if (passo.Esamina(riga) is { } fallito)
                return fallito;
        }

        return passo.Esito();
    }

    private sealed class Passo
    {
        private byte[] _attesa = ImprontaEvento.Iniziale;
        private long _verificati;

        public EsitoVerifica? Esamina(EventoRegistro riga)
        {
            if (!riga.ImprontaPrecedente.AsSpan().SequenceEqual(_attesa))
                return Fallito(riga, MotivoPrecedente);

            NuovoEvento evento;
            try
            {
                evento = riga.ComeNuovoEvento();
            }
            catch (ArgumentException)
            {
                // Una riga che passa i CHECK ma non il dominio è un'alterazione da segnalare, non un errore.
                return Fallito(riga, MotivoRigaNonValida);
            }

            var ricalcolata = ImprontaEvento.Calcola(riga.ImprontaPrecedente, evento);
            if (!ricalcolata.AsSpan().SequenceEqual(riga.Impronta))
                return Fallito(riga, MotivoRicalcolata);

            _attesa = riga.Impronta;
            _verificati++;
            return null;
        }

        public EsitoVerifica Esito() => new(true, _verificati, null, null);

        private EsitoVerifica Fallito(EventoRegistro riga, string motivo) => new(false, _verificati, riga.Id, motivo);
    }
}
