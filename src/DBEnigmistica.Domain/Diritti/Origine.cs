namespace DBEnigmistica.Domain.Diritti;

/// <summary>Da dove viene un Gioco; senza valore zero, così il default non è mai un'origine valida.</summary>
public enum Origine
{
    Originale = 1,
    Licenza = 2,
    PubblicoDominio = 3,
    Rivista = 4
}
