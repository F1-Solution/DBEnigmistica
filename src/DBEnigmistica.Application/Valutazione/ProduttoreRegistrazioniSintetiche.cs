using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>
/// Ricalcola le Registrazioni e la Linea di base di un Set sintetico dalle risposte in <c>risposte/&lt;id&gt;.json</c>;
/// rifiuta i set di riferimento.
/// </summary>
public sealed class ProduttoreRegistrazioniSintetiche(
    RisolutorePagineSet risolutore,
    IVersionePromptEstrazione versionePrompt,
    IFabbricaArchivioRegistrazioni fabbrica,
    IInterpreteRispostaGrezza interprete,
    IValidatoreSchemaJson validatore,
    ServizioRegressioneEstrazione servizio)
{
    // Data fissa: le Registrazioni sintetiche sono identiche a ogni ricalcolo.
    private static readonly DateTimeOffset DataRegistrazione = new(2026, 11, 24, 0, 0, 0, TimeSpan.Zero);

    public async Task<int> AggiornaAsync(
        string cartellaSet, string fornitore, string modello, int dpi, bool scriviLineaDiBase = true, CancellationToken cancellationToken = default)
    {
        var set = LettoreSet.Leggi(cartellaSet);
        if (set.Tipo != TipoSet.Sintetico)
            throw new SetNonValidoException("Operazione rifiutata: il set è di tipo riferimento, le sue Registrazioni non si ricalcolano.");

        var risolte = await risolutore.RisolviAsync(set, null, cancellationToken);
        var risposte = risolte.Select(r => (r, Grezza: LeggiRisposta(set, r.Pagina.Id))).ToList();

        // Le chiavi vecchie non servono più: la cartella si ricrea per intero.
        var cartellaRegistrazioni = Path.Combine(set.Cartella, "registrazioni");
        if (Directory.Exists(cartellaRegistrazioni))
            Directory.Delete(cartellaRegistrazioni, recursive: true);

        var archivio = fabbrica.Apri(cartellaRegistrazioni);
        foreach (var (risolta, grezza) in risposte)
        {
            var chiave = new ChiaveRegistrazione(risolta.Impronta, versionePrompt.VersionePrompt, fornitore, modello);
            await archivio.SalvaAsync(
                new Registrazione(
                    chiave, DataRegistrazione, dpi, EsitoEstrazione.Riuscita, grezza,
                    Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(grezza))),
                    new MetadatiChiamata(0, null, null, null, null, null, null, null)),
                cancellationToken);
        }

        if (scriviLineaDiBase)
        {
            // La Linea di base è la misura delle Registrazioni appena scritte.
            var riproduzione = new FornitoreAiRiproduci(fornitore, modello, archivio, interprete, validatore);
            var rapporto = await servizio.MisuraAsync(new ParametriRegressione(set.Cartella, fornitore, modello, dpi), riproduzione, cancellationToken);
            await File.WriteAllTextAsync(
                Path.Combine(set.Cartella, ServizioRegressioneEstrazione.NomeLineaDiBase), LineaDiBase.DaRapporto(rapporto).InJson(),
                new UTF8Encoding(false), cancellationToken);
        }

        return risposte.Count;
    }

    // Avvolge la risposta nel formato dell'uscita di Claude Code, compatta e senza spazi superflui.
    private static string LeggiRisposta(SetValutazione set, string id)
    {
        var percorso = Path.Combine(set.Cartella, "risposte", id + ".json");
        if (!File.Exists(percorso))
            throw new SetNonValidoException($"Pagina {id}: manca la risposta (risposte/{id}.json).");

        try
        {
            using var documento = JsonDocument.Parse(File.ReadAllText(percorso));
            using var memoria = new MemoryStream();
            using (var w = new Utf8JsonWriter(memoria))
            {
                w.WriteStartObject();
                w.WriteString("type", "result");
                w.WriteBoolean("is_error", false);
                w.WriteString("result", string.Empty);
                w.WritePropertyName("structured_output");
                documento.RootElement.WriteTo(w);
                w.WriteEndObject();
            }

            return Encoding.UTF8.GetString(memoria.ToArray());
        }
        catch (JsonException ex)
        {
            throw new SetNonValidoException($"Pagina {id}: la risposta non è un JSON valido.", ex);
        }
    }
}
