namespace DBEnigmistica.Domain.Enums;

/// <summary>
/// Tipologie di giochi enigmistici supportate
/// </summary>
public enum GameType
{
    Unknown,
    Crossword,        // Cruciverba classico
    Rebus,            // Rebus
    Logic,            // Giochi di logica
    Anagram,          // Anagrammi
    WordSearch,       // Trova parole
    Sudoku,           // Sudoku
    Cryptic,          // Crittografia
    Quiz,             // Quiz
    Charade,          // Sciarada
    Riddle            // Indovinello
}
