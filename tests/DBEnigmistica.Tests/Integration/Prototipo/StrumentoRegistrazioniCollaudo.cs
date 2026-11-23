using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Infrastructure.Prototipo;
using DBEnigmistica.Tests.Unit.Creazione;

namespace DBEnigmistica.Tests.Integration.Prototipo;

/// <summary>Il test dell'attributo gira solo con <c>DBE_AGGIORNA_REGISTRAZIONI=1</c>: è uno strumento, non una prova.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class StrumentoAttribute : FactAttribute
{
    public const string Variabile = "DBE_AGGIORNA_REGISTRAZIONI";

    public StrumentoAttribute()
    {
        if (Environment.GetEnvironmentVariable(Variabile) != "1")
            Skip = $"Strumento: imposta {Variabile}=1 per riscrivere le Registrazioni del collaudo.";
    }
}

/// <summary>
/// Riscrive le Registrazioni sintetiche del collaudo (testi inventati, nessuna chiamata) partendo dai brief in <c>brief/</c>,
/// tali e quali li invia la schermata. Si esegue a mano: <c>DBE_AGGIORNA_REGISTRAZIONI=1 dotnet test --filter Category=Strumento</c>.
/// </summary>
[Trait("Category", "Strumento")]
public sealed class StrumentoRegistrazioniCollaudo
{
    public static readonly string[] Codici = ["crit-sin-collaudo", "crit-per-collaudo", "anag-collaudo", "rebus-collaudo", "cruci-collaudo"];

    public const string Fornitore = "claude-code";
    public const string ModelloAi = "claude-opus-5-5";

    public static string CartellaFixture => FileDelRepository.Percorso("tests", "DBEnigmistica.Tests", "Fixtures", "RegistrazioniCreazione");

    public static string Brief(string codice) => File.ReadAllText(Path.Combine(CartellaFixture, "brief", codice + ".json")).Replace("\r\n", "\n", StringComparison.Ordinal);

    [Strumento]
    public async Task RiscriviLeRegistrazioni()
    {
        var archivio = new ArchivioRegistrazioniCreazioneFileSystem(CartellaFixture);
        var catalogo = new CatalogoTipologieFileSystem(FileDelRepository.Percorso("tipologie"));
        var modello = ModelloPromptCreazione.Carica(RisorseCreazione.PromptCreazione);
        foreach (var codice in Codici)
        {
            // Come la generazione: brief della schermata con la variante 1; il cruciverba usa lo schema del seme di riproduzione.
            var brief = BriefCreazione.Leggi(Brief(codice)).ConVariante(1);
            var descrittore = (await catalogo.TrovaAsync(brief.Tipologia, brief.VersioneTipologia))!;
            SchemaCaselle? caselle = descrittore.Famiglia == "cruciverba"
                ? new GeneratoriAlgoritmici().GeneraSchemaCaselle(brief.Dimensioni!.Righe, brief.SemeDiRiproduzione())
                : null;
            var richiesta = modello.Compila(descrittore, brief, caselle, consentiBozza: true);
            var risposta = JsonNode.Parse(Risposta(codice, caselle))!.ToJsonString();
            var grezza = "{\"type\":\"result\",\"subtype\":\"success\",\"is_error\":false,\"result\":\"\",\"structured_output\":" + risposta
                         + ",\"total_cost_usd\":0,\"usage\":{\"input_tokens\":0,\"output_tokens\":0}}";
            await archivio.SalvaAsync(
                new RegistrazioneCreazione(
                    new ChiaveRegistrazione(richiesta.ImprontaRichiesta, richiesta.VersionePrompt, Fornitore, ModelloAi),
                    new DateTimeOffset(2026, 12, 1, 9, 0, 0, TimeSpan.Zero),
                    richiesta.Tipologia, richiesta.VersioneTipologia, EsitoEstrazione.Riuscita, grezza,
                    Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(grezza))),
                    new MetadatiChiamata(1000, null, null, null, null, null, null, null)));
        }
    }

    // Risposta inventata per ogni brief: valida per lo schema e per i validatori, senza testo di Riviste.
    private static string Risposta(string codice, SchemaCaselle? caselle) => codice switch
    {
        "crit-sin-collaudo" => FileDelRepository.Esempio("risposta", "valido-crittografia-sinonimica.json"),
        "crit-per-collaudo" => """
            {"versione_formato":1,"tipologia":"crittografia-breve","versione_tipologia":0,"titolo":null,
             "dati":{"sottotipo":"perifrastica","esposto":"TERZA CIFRA E RIFIUTO","diagramma":"3 2 = 5","prima_lettura":"TRE NO","soluzione":"TRENO","spiegazione":"Terza cifra = TRE; rifiuto = NO; TRE NO = TRENO."},
             "avvisi":[]}
            """,
        "anag-collaudo" => """
            {"versione_formato":1,"tipologia":"anagramma","versione_tipologia":1,"titolo":"Sulla riva",
             "dati":{"esposto":"Si stende all'orizzonte, azzurra e profonda;\nrimescolata, dà il colore ai tetti e ai vasi.","diagramma":"4 = 4","prima_parola":"mare","soluzione":"rame","autore":null},
             "avvisi":["Esempio inventato per il collaudo."]}
            """,
        "rebus-collaudo" => FileDelRepository.Esempio("risposta", "valido-rebus.json"),
        "cruci-collaudo" => Cruciverba(caselle!),
        _ => throw new ArgumentOutOfRangeException(nameof(codice)),
    };

    // Lettere inventate e senza senso, ma coerenti con lo schema: griglia, numeri e soluzioni delle definizioni combaciano.
    private static string Cruciverba(SchemaCaselle caselle)
    {
        const string lettere = "ARENOSITCLMPD";
        var celle = caselle.Celle.Select((riga, r) => new string([.. riga.Select((c, k) => c == '#' ? '#' : lettere[((r * 7) + (k * 3) + (r * k)) % lettere.Length])])).ToList();
        var nodo = JsonNode.Parse(FileDelRepository.Esempio("risposta", "valido-esempio-cruciverba-9x9.json"))!;
        nodo["tipologia"] = "cruciverba-classico";
        nodo["versione_tipologia"] = 1;
        var dati = nodo["dati"]!;
        dati["righe"] = caselle.Righe;
        dati["colonne"] = caselle.Colonne;
        var griglia = dati["griglia"]!;
        griglia["righe"] = caselle.Righe;
        griglia["colonne"] = caselle.Colonne;
        griglia["celle"] = new JsonArray([.. celle.Select(c => (JsonNode)c)]);
        griglia["numeri"] = new JsonArray([.. caselle.Numeri.Select(n => (JsonNode)new JsonObject { ["riga"] = n.Riga, ["colonna"] = n.Colonna, ["numero"] = n.Numero })]);
        dati["definizioni"] = new JsonArray([.. caselle.Parole.Select(p => (JsonNode)new JsonObject
        {
            ["numero"] = p.Numero,
            ["direzione"] = p.Direzione,
            ["testo"] = $"Definizione inventata {p.Numero} {p.Direzione}",
            ["soluzione"] = Lettere(celle, p),
        })]);
        nodo["avvisi"] = new JsonArray("Registrazione sintetica per il collaudo: lettere casuali, valida solo per lo schema.");
        return nodo.ToJsonString();
    }

    private static string Lettere(IReadOnlyList<string> celle, ParolaSchema p)
    {
        var testo = new char[p.Lunghezza];
        for (var i = 0; i < p.Lunghezza; i++)
        {
            var r = p.Riga - 1 + (p.Direzione == SchemaCaselle.Verticale ? i : 0);
            var c = p.Colonna - 1 + (p.Direzione == SchemaCaselle.Orizzontale ? i : 0);
            testo[i] = celle[r][c];
        }

        return new string(testo);
    }
}
