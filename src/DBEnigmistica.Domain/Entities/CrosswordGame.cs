namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Rappresenta un cruciverba
/// </summary>
public class CrosswordGame
{
    public int Id { get; set; }
    
    /// <summary>
    /// Riferimento al gioco base
    /// </summary>
    public int GameId { get; set; }
    
    /// <summary>
    /// Numero di righe della griglia
    /// </summary>
    public int Rows { get; set; }
    
    /// <summary>
    /// Numero di colonne della griglia
    /// </summary>
    public int Columns { get; set; }
    
    /// <summary>
    /// Struttura della griglia (JSON) - matrice di celle con numeri e caselle nere
    /// Format: {"cells": [[{"number": 1, "black": false}, ...], ...]}
    /// </summary>
    public string? GridStructure { get; set; }
    
    /// <summary>
    /// Definizioni orizzontali (JSON)
    /// Format: {"1": "Prima definizione orizzontale", "5": "Seconda...", ...}
    /// </summary>
    public string? HorizontalClues { get; set; }
    
    /// <summary>
    /// Definizioni verticali (JSON)
    /// Format: {"1": "Prima definizione verticale", "2": "Seconda...", ...}
    /// </summary>
    public string? VerticalClues { get; set; }
    
    /// <summary>
    /// Soluzione completa del cruciverba (se disponibile)
    /// </summary>
    public string? Solution { get; set; }
    
    /// <summary>
    /// Indica se è un cruciverba sillabico
    /// </summary>
    public bool IsSyllabic { get; set; }
    
    /// <summary>
    /// Indica se è un cruciverba a schema libero
    /// </summary>
    public bool IsFreeSchema { get; set; }
    
    // Navigation property
    public Game Game { get; set; } = null!;
}
