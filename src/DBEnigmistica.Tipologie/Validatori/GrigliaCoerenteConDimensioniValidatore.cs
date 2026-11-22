using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Righe e colonne dei dati coincidono con la griglia, che è rettangolare e usa solo i simboli ammessi.</summary>
public sealed class GrigliaCoerenteConDimensioniValidatore : ValidatoreStrutturaleBase
{
    public override NomeValidatore Nome => NomeValidatore.GrigliaCoerenteConDimensioni;

    public override IReadOnlyList<RuoloCampo> RuoliRichiesti { get; } = [RuoloCampo.Righe, RuoloCampo.Colonne, RuoloCampo.Griglia];

    protected override void Controlla(Descrittore descrittore, JsonElement dati, List<ProblemaValidazione> problemi)
    {
        var nomeRighe = NomeCampo(descrittore, RuoloCampo.Righe);
        var nomeColonne = NomeCampo(descrittore, RuoloCampo.Colonne);
        var nomeGriglia = NomeCampo(descrittore, RuoloCampo.Griglia);
        var righe = LetturaDati.Intero(dati, nomeRighe);
        var colonne = LetturaDati.Intero(dati, nomeColonne);
        if (righe is null)
            problemi.Add(new ProblemaValidazione(nomeRighe, "Le righe devono essere un numero intero."));
        if (colonne is null)
            problemi.Add(new ProblemaValidazione(nomeColonne, "Le colonne devono essere un numero intero."));
        if (LetturaDati.Elemento(dati, nomeGriglia) is not { } valore)
        {
            problemi.Add(new ProblemaValidazione(nomeGriglia, "La griglia manca."));
            return;
        }

        if (!GrigliaLetta.TryLeggi(valore, verificaForma: false, out var griglia, out var errore))
        {
            problemi.Add(new ProblemaValidazione(nomeGriglia, errore));
            return;
        }

        if (righe is { } r && r != griglia!.Righe)
            problemi.Add(new ProblemaValidazione(nomeRighe, $"Le righe ({r}) sono diverse da quelle della griglia ({griglia.Righe})."));
        if (colonne is { } c && c != griglia!.Colonne)
            problemi.Add(new ProblemaValidazione(nomeColonne, $"Le colonne ({c}) sono diverse da quelle della griglia ({griglia.Colonne})."));

        ControllaCelle(griglia!, descrittore.Resa.Modello == ModelloResa.GrigliaLogica, nomeGriglia, problemi);
        ControllaNumeri(griglia!, nomeGriglia, problemi);
    }

    private static void ControllaCelle(GrigliaLetta griglia, bool ammetteCifre, string nomeGriglia, List<ProblemaValidazione> problemi)
    {
        if (griglia.Celle.Count != griglia.Righe)
            problemi.Add(new ProblemaValidazione($"{nomeGriglia}.celle", $"La griglia ha {griglia.Celle.Count} righe di celle invece di {griglia.Righe}."));
        for (var i = 0; i < griglia.Celle.Count; i++)
        {
            var riga = griglia.Celle[i];
            if (riga.Length != griglia.Colonne)
                problemi.Add(new ProblemaValidazione($"{nomeGriglia}.celle[{i}]", $"La riga ha {riga.Length} caselle invece di {griglia.Colonne}."));
            else if (riga.Any(ch => !SimboloAmmesso(ch, ammetteCifre)))
                problemi.Add(new ProblemaValidazione($"{nomeGriglia}.celle[{i}]", "La riga contiene un simbolo non ammesso: servono #, ., ? e lettere maiuscole A-Z."));
        }
    }

    private static void ControllaNumeri(GrigliaLetta griglia, string nomeGriglia, List<ProblemaValidazione> problemi)
    {
        for (var i = 0; i < griglia.Numeri.Count; i++)
        {
            var n = griglia.Numeri[i];
            var percorso = $"{nomeGriglia}.numeri[{i}]";
            if (n.Riga < 1 || n.Riga > griglia.Celle.Count || n.Colonna < 1 || n.Colonna > griglia.Celle[n.Riga - 1].Length)
                problemi.Add(new ProblemaValidazione(percorso, "Il numero è fuori dalla griglia."));
            else if (griglia.Celle[n.Riga - 1][n.Colonna - 1] == '#')
                problemi.Add(new ProblemaValidazione(percorso, "Il numero è su una casella nera."));
        }
    }

    private static bool SimboloAmmesso(char c, bool ammetteCifre) =>
        c is '#' or '.' or '?' or (>= 'A' and <= 'Z') || (ammetteCifre && c is >= '0' and <= '9');
}
