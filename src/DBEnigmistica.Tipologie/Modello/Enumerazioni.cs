namespace DBEnigmistica.Tipologie.Modello;

/// <summary>Famiglie chiuse del meta-schema.</summary>
public enum FamigliaTipologia
{
    Cruciverba,
    GiocoBreve,
    Crittografia,
    Rebus,
    Logico,
}

/// <summary>Tipi dei campi (ADR 002, punto 4).</summary>
public enum TipoCampo
{
    Testo,
    TestoLungo,
    Intero,
    Decimale,
    Booleano,
    Data,
    Enumerazione,
    Griglia,
    Immagine,
}

/// <summary>Funzione di un campo nella resa e nelle metriche.</summary>
public enum RuoloCampo
{
    Righe,
    Colonne,
    Griglia,
    Numero,
    Direzione,
    Definizione,
    Esposto,
    Diagramma,
    Soluzione,
    Immagine,
}

/// <summary>Modelli di resa del Lettore (ADR 002, punto 11).</summary>
public enum ModelloResa
{
    Griglia,
    EspostoDiagramma,
    ImmagineDiagramma,
    GrigliaLogica,
}

/// <summary>Stato delle regole del gioco.</summary>
public enum StatoRegole
{
    Bozza,
    Approvata,
}

/// <summary>Validatori strutturali dell'elenco chiuso del meta-schema.</summary>
public enum NomeValidatore
{
    GrigliaCoerenteConDimensioni,
    NumerazioneCoerenteConGriglia,
    LunghezzeCoerenti,
    IncrociCoerenti,
    DiagrammaCoerenteConSoluzione,
    AnagrammaStesseLettere,
    CambioDiUnaLettera,
    SciaradaPartiCoerenti,
}
