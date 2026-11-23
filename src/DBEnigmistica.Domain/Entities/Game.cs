using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Enums;

namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Entità base per un gioco enigmistico
/// </summary>
public class Game
{
    public int Id { get; set; }

    /// <summary>
    /// Riferimento alla pagina
    /// </summary>
    public int PageId { get; set; }

    /// <summary>Identificativo della Tipologia del Gioco.</summary>
    public required string Tipologia { get; set; }

    /// <summary>Versione del descrittore della Tipologia.</summary>
    public int VersioneTipologia { get; set; } = 1;

    /// <summary>Tipologia nota oppure proposta dall'estrazione.</summary>
    public StatoTipologia StatoTipologia { get; set; } = StatoTipologia.Nota;

    /// <summary>Titolo del gioco (se presente)</summary>
    public string? Title { get; set; }

    /// <summary>Riquadro nella pagina, normalizzato 0-1: ascissa dell'angolo in alto a sinistra.</summary>
    public decimal? RiquadroX { get; set; }

    /// <summary>Riquadro nella pagina, normalizzato 0-1: ordinata dell'angolo in alto a sinistra.</summary>
    public decimal? RiquadroY { get; set; }

    /// <summary>Larghezza del riquadro, normalizzata 0-1.</summary>
    public decimal? RiquadroLarghezza { get; set; }

    /// <summary>Altezza del riquadro, normalizzata 0-1.</summary>
    public decimal? RiquadroAltezza { get; set; }

    /// <summary>Avvisi dell'estrazione, come array JSON.</summary>
    public string Avvisi { get; set; } = "[]";

    /// <summary>Testo visibile di un Gioco di Tipologia proposta; assente se la Tipologia è nota.</summary>
    public string? TrascrizioneProposta { get; set; }

    /// <summary>
    /// Livello di difficoltà (se disponibile)
    /// </summary>
    public string? Difficulty { get; set; }

    /// <summary>
    /// Note o descrizione aggiuntiva
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Data di estrazione
    /// </summary>
    public DateTime ExtractedDate { get; set; }

    /// <summary>Confidenza dell'estrazione per il Gioco (0-1).</summary>
    public double? Confidenza { get; set; }

    /// <summary>
    /// Soluzione breve del gioco (per soluzioni semplici)
    /// </summary>
    public string? Solution { get; set; }

    /// <summary>
    /// Riferimento opzionale alla pagina contenente la soluzione
    /// </summary>
    public int? SolutionPageId { get; set; }

    /// <summary>
    /// Indica se il gioco ha una soluzione disponibile
    /// </summary>
    public bool HasSolution { get; set; }

    /// <summary>
    /// Note aggiuntive sulla soluzione
    /// </summary>
    public string? SolutionNotes { get; set; }

    /// <summary>
    /// Origine del Gioco: per un Gioco estratto è sempre Rivista
    /// </summary>
    public required Origine Origine { get; init; }

    // Navigation properties
    public Page Page { get; set; } = null!;
    public Page? SolutionPage { get; set; }
}
