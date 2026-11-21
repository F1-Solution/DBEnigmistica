using System.Security.Cryptography;
using System.Text;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Registro;
using DBEnigmistica.Domain.Registro;
using DBEnigmistica.Infrastructure.Data;
using DBEnigmistica.Infrastructure.Prototipo;
using DBEnigmistica.Infrastructure.Repositories;
using DBEnigmistica.Tests.Unit.Creazione;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Tests.Integration.Prototipo;

/// <summary>Descrittori in memoria: nei test le regole sono in bozza e solo i casi d'uso di prova le ammettono.</summary>
public sealed class CatalogoInMemoria(params DescrittoreTipologia[] descrittori) : ICatalogoTipologie
{
    public Task<DescrittoreTipologia?> TrovaAsync(string id, int versione, CancellationToken cancellationToken = default) =>
        Task.FromResult(descrittori.FirstOrDefault(d => d.Id == id && d.Versione == versione));
}

/// <summary>Fornitore di creazione finto: restituisce le risposte preparate in ordine, senza alcuna chiamata, e ricorda le richieste.</summary>
public sealed class FornitoreCreazioneFinto : IFornitoreAiCreazione
{
    private readonly Queue<Func<RichiestaCreazione, RispostaCreazione>> _risposte = new();

    public string IdFornitore => "finto";

    public string Modello => "modello-finto";

    public List<RichiestaCreazione> Richieste { get; } = [];

    public FornitoreCreazioneFinto Riuscita(string json) => Accoda(r => Risposta(r, EsitoEstrazione.Riuscita, json, null));

    public FornitoreCreazioneFinto NonValida(string messaggio = "campo mancante") => Accoda(r => Risposta(r, EsitoEstrazione.RispostaNonValida, null, messaggio));

    public FornitoreCreazioneFinto FinestraEsaurita() => Accoda(r => Risposta(r, EsitoEstrazione.FinestraEsaurita, null, "Finestra d'uso esaurita"));

    private FornitoreCreazioneFinto Accoda(Func<RichiestaCreazione, RispostaCreazione> risposta)
    {
        _risposte.Enqueue(risposta);
        return this;
    }

    public Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default)
    {
        Richieste.Add(richiesta);
        return Task.FromResult(_risposte.Dequeue()(richiesta));
    }

    private RispostaCreazione Risposta(RichiestaCreazione r, EsitoEstrazione esito, string? json, string? messaggio)
    {
        var grezza = "{\"type\":\"result\",\"testo\":\"RISPOSTA-GREZZA-DI-PROVA\"}";
        var chiamata = new ChiamataAi(
            Guid.CreateVersion7(), DateTimeOffset.UtcNow, 12, ChiamataAi.UsoCreazione, IdFornitore, Modello, r.VersionePrompt,
            r.ImprontaByte(), 1, false, esito, null, null, esito == EsitoEstrazione.Riuscita ? null : messaggio, 100, 20, null, null,
            0.5m, "USD", ChiamataAi.CostoIndicativo, false, grezza, SHA256.HashData(Encoding.UTF8.GetBytes(grezza)));
        return new RispostaCreazione(esito, json, messaggio, null, [chiamata], MetadatiChiamata.Somma([chiamata]));
    }
}

/// <summary>Generatori finti: schema del cruciverba d'esempio e contenuti fissi.</summary>
public sealed class GeneratoriFinti(SchemaCaselle? caselle = null) : IGeneratoriAlgoritmici
{
    private readonly GeneratoriAlgoritmici _veri = new();

    public bool Supporta(string tipologia) => _veri.Supporta(tipologia);

    public string VersioneGeneratore(string tipologia) => _veri.VersioneGeneratore(tipologia);

    public RisultatoAlgoritmico Genera(string tipologia, BriefCreazione brief, long seme) => _veri.Genera(tipologia, brief, seme);

    public SchemaCaselle GeneraSchemaCaselle(int dimensione, long seme) => caselle ?? _veri.GeneraSchemaCaselle(dimensione, seme);
}

/// <summary>Semi fissi, uno dopo l'altro.</summary>
public sealed class SemiFissi : ISorgenteSemi
{
    private long _prossimo = 1000;

    public long Prossimo() => _prossimo++;
}

/// <summary>Un insieme di casi d'uso sullo stesso contesto, che lavora con il ruolo <c>dbe_creazione</c> come l'host.</summary>
public sealed class ServiziPrototipo : IAsyncDisposable
{
    public required EnigmisticaDbContext Contesto { get; init; }

    public required GeneraCandidato Genera { get; init; }

    public required RevisioneCandidati Revisione { get; init; }

    public required CandidatiRepository Repository { get; init; }

    public required RegistroEventiRepository Registro { get; init; }

    public ValueTask DisposeAsync() => Contesto.DisposeAsync();
}

public abstract class PrototipoTestBase(PostgresFixture fixture) : IntegrationTestBase(fixture)
{
    protected static DescrittoreTipologia Descrittore(string id) =>
        DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", id, "v0.json"));

    protected static string BriefCrittografia() => FileDelRepository.Esempio("brief", "valido-crittografia-sinonimica.json");

    protected static string RispostaCrittografia(string esposto = "CENA SOVRANO", string spiegazione = "CENA = PASTO; SOVRANO = RE; PASTO RE = PASTORE.") =>
        FileDelRepository.Esempio("risposta", "valido-crittografia-sinonimica.json")
            .Replace("CENA SOVRANO", esposto, StringComparison.Ordinal)
            .Replace("CENA = PASTO; SOVRANO = RE; PASTO RE = PASTORE.", spiegazione, StringComparison.Ordinal);

    protected ServiziPrototipo Servizi(
        IFornitoreAiCreazione? fornitore = null,
        ICatalogoTipologie? catalogo = null,
        IGeneratoriAlgoritmici? generatori = null,
        TimeProvider? tempo = null,
        bool consentiBozza = true)
    {
        var opzioni = new DbContextOptionsBuilder<EnigmisticaDbContext>()
            .UseNpgsql(ConnectionString)
            .AddInterceptors(new RuoloCreazioneInterceptor())
            .Options;
        var contesto = new EnigmisticaDbContext(opzioni);
        var repository = new CandidatiRepository(contesto);
        var registro = new RegistroEventiRepository(contesto);
        var transazione = new TransazionePrototipo(contesto);
        var validatore = new DBEnigmistica.AI.Services.ValidatoreSchemaJson();
        catalogo ??= new CatalogoInMemoria(Descrittore("crittografia-breve"), Descrittore("rebus"), Descrittore("sudoku"), Descrittore("futoshiki"), Descrittore("nonogramma"));
        generatori ??= new GeneratoriFinti();
        var modello = ModelloPromptCreazione.Carica(RisorseCreazione.PromptCreazione);
        var brief = new ValidatoreBrief(validatore, generatori.Supporta);
        return new ServiziPrototipo
        {
            Contesto = contesto,
            Repository = repository,
            Registro = registro,
            Genera = new GeneraCandidato(
                catalogo, brief, modello, fornitore ?? new FornitoreCreazioneFinto(), validatore, generatori, new SemiFissi(),
                repository, registro, transazione, tempo, consentiRegoleInBozza: consentiBozza),
            Revisione = new RevisioneCandidati(catalogo, validatore, repository, registro, transazione, tempo),
        };
    }

    protected async Task<EsitoVerifica> VerificaRegistroAsync()
    {
        await using var context = NewContext();
        return await new VerificaIntegritaRegistro(new RegistroEventiRepository(context)).EseguiAsync();
    }
}
