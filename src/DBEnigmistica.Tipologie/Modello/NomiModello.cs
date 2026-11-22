namespace DBEnigmistica.Tipologie.Modello;

/// <summary>Corrispondenza fra i valori delle enumerazioni e i nomi che compaiono nei descrittori JSON.</summary>
public static class NomiModello
{
    private static readonly Dictionary<Type, (Dictionary<object, string> DaValore, Dictionary<string, object> DaNome)> Tabelle = new()
    {
        [typeof(FamigliaTipologia)] = Crea(
            (FamigliaTipologia.Cruciverba, "cruciverba"), (FamigliaTipologia.GiocoBreve, "gioco-breve"),
            (FamigliaTipologia.Crittografia, "crittografia"), (FamigliaTipologia.Rebus, "rebus"), (FamigliaTipologia.Logico, "logico")),
        [typeof(TipoCampo)] = Crea(
            (TipoCampo.Testo, "testo"), (TipoCampo.TestoLungo, "testo_lungo"), (TipoCampo.Intero, "intero"),
            (TipoCampo.Decimale, "decimale"), (TipoCampo.Booleano, "booleano"), (TipoCampo.Data, "data"),
            (TipoCampo.Enumerazione, "enumerazione"), (TipoCampo.Griglia, "griglia"), (TipoCampo.Immagine, "immagine")),
        [typeof(RuoloCampo)] = Crea(
            (RuoloCampo.Righe, "righe"), (RuoloCampo.Colonne, "colonne"), (RuoloCampo.Griglia, "griglia"),
            (RuoloCampo.Numero, "numero"), (RuoloCampo.Direzione, "direzione"), (RuoloCampo.Definizione, "definizione"),
            (RuoloCampo.Esposto, "esposto"), (RuoloCampo.Diagramma, "diagramma"), (RuoloCampo.Soluzione, "soluzione"),
            (RuoloCampo.Immagine, "immagine")),
        [typeof(ModelloResa)] = Crea(
            (ModelloResa.Griglia, "griglia"), (ModelloResa.EspostoDiagramma, "esposto_diagramma"),
            (ModelloResa.ImmagineDiagramma, "immagine_diagramma"), (ModelloResa.GrigliaLogica, "griglia_logica")),
        [typeof(StatoRegole)] = Crea((StatoRegole.Bozza, "bozza"), (StatoRegole.Approvata, "approvata")),
        [typeof(NomeValidatore)] = Crea(
            (NomeValidatore.GrigliaCoerenteConDimensioni, "griglia-coerente-con-dimensioni"),
            (NomeValidatore.NumerazioneCoerenteConGriglia, "numerazione-coerente-con-griglia"),
            (NomeValidatore.LunghezzeCoerenti, "lunghezze-coerenti"),
            (NomeValidatore.IncrociCoerenti, "incroci-coerenti"),
            (NomeValidatore.DiagrammaCoerenteConSoluzione, "diagramma-coerente-con-soluzione"),
            (NomeValidatore.AnagrammaStesseLettere, "anagramma-stesse-lettere"),
            (NomeValidatore.CambioDiUnaLettera, "cambio-di-una-lettera"),
            (NomeValidatore.SciaradaPartiCoerenti, "sciarada-parti-coerenti")),
    };

    /// <summary>Nome del valore nel descrittore JSON.</summary>
    public static string Nome<T>(T valore) where T : struct, Enum => Tabelle[typeof(T)].DaValore[valore];

    /// <summary>Legge un nome del descrittore JSON; i nomi sono sensibili alle maiuscole.</summary>
    public static bool TryLeggi<T>(string? nome, out T valore) where T : struct, Enum
    {
        if (nome is not null && Tabelle[typeof(T)].DaNome.TryGetValue(nome, out var trovato))
        {
            valore = (T)trovato;
            return true;
        }

        valore = default;
        return false;
    }

    private static (Dictionary<object, string>, Dictionary<string, object>) Crea<T>(params (T Valore, string Nome)[] coppie)
        where T : struct, Enum =>
        (coppie.ToDictionary(c => (object)c.Valore, c => c.Nome),
         coppie.ToDictionary(c => c.Nome, c => (object)c.Valore, StringComparer.Ordinal));
}
