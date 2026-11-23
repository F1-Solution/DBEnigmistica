# Brief del collaudo 1b.2 per le famiglie con AI

Cinque brief fissi, uno o due per famiglia, con le Registrazioni sintetiche che li riproducono (testi inventati, nessuna chiamata dal vivo). I file stanno in `tests/DBEnigmistica.Tests/Fixtures/RegistrazioniCreazione/`: i brief in `brief/<nome>.json` (il nome è solo l'etichetta del brief), le Registrazioni in `claude-code/claude-opus-5-5/`.

## Avvio del prototipo in riproduzione

Variabili d'ambiente (nomi, nessun segreto): `ConnectionStrings__DefaultConnection` (stringa di connessione), `FornitoreAi__Tipo=claude-code`, `FornitoreAi__Creazione__Modello=claude-opus-5-5`, `FornitoreAi__Creazione__Registrazioni__Modalita=riproduci`, `FornitoreAi__Creazione__Registrazioni__Cartella=<percorso assoluto di tests\DBEnigmistica.Tests\Fixtures\RegistrazioniCreazione>`. `FornitoreAi__DalVivoAbilitato` resta `false`. Poi `dbe prototipo`.

In riproduzione le regole in bozza sono ammesse e *Genera* è abilitato per le quattro famiglie. Il modello deve essere esattamente `claude-opus-5-5`: la Registrazione si trova con fornitore, modello e impronta della richiesta. Con un brief diverso da questi, o dopo una modifica alle regole o ai descrittori, la Registrazione manca e *Genera* risponde con un errore.

Ogni brief si usa dalla schermata "Nuovo candidato" con i valori sotto, una sola volta: un secondo clic con gli stessi valori cambia la variante e non trova più la Registrazione.

## I brief

I campi non indicati non esistono nel modulo. La variante la assegna il programma.

### crit-sin-collaudo (crittografia breve, sinonimica)

| Campo | Valore |
|---|---|
| Tipologia | `crittografia-breve` (versione 0, famiglia `crittografia`) |
| Sottotipo | `sinonimica` |
| Tema | `il mercato` |
| Vincoli | `Esposto di al massimo 2 parole` |
| Difficoltà | 2 (facile) |
| Dimensioni | nessuna |

Candidato prodotto: esposto `CENA SOVRANO`, diagramma `5 2 = 7`, soluzione `PASTORE`.

### crit-per-collaudo (crittografia breve, perifrastica)

| Campo | Valore |
|---|---|
| Tipologia | `crittografia-breve` (versione 0, famiglia `crittografia`) |
| Sottotipo | `perifrastica` |
| Tema | vuoto (tema libero) |
| Vincoli | nessuno |
| Difficoltà | 3 (media) |
| Dimensioni | nessuna |

Candidato prodotto: esposto `TERZA CIFRA E RIFIUTO`, diagramma `3 2 = 5`, soluzione `TRENO`.

### anag-collaudo (anagramma)

| Campo | Valore |
|---|---|
| Tipologia | `anagramma` (versione 1, famiglia `gioco-breve`) |
| Tema | `il mare` |
| Vincoli | `Anagramma semplice: una parola diventa un'altra parola` e `Esposto di 2-4 versi` (due righe) |
| Difficoltà | 2 (facile) |
| Dimensioni | nessuna |

Candidato prodotto: prima parola `mare`, soluzione `rame`, diagramma `4 = 4`.

### rebus-collaudo (rebus)

| Campo | Valore |
|---|---|
| Tipologia | `rebus` (versione 0, famiglia `rebus`) |
| Tema | vuoto (tema libero) |
| Vincoli | `Una sola vignetta` e `Al massimo 3 figure` (due righe) |
| Difficoltà | 2 (facile) |
| Dimensioni | nessuna |

Candidato prodotto: soluzione `STRANA IDEA`, diagramma `6 4`, due vignette descritte a parole.

### cruci-collaudo (cruciverba)

| Campo | Valore |
|---|---|
| Tipologia | `cruciverba-classico` (versione 1, famiglia `cruciverba`) |
| Tema | vuoto (tema libero) |
| Vincoli | `Definizioni brevi, al massimo 8 parole` |
| Difficoltà | 2 (facile) |
| Dimensioni | quadrate, 9 righe per 9 colonne |

Candidato prodotto: griglia 9x9 con lettere inventate e senza senso (valide solo per lo schema), definizioni del tipo `Definizione inventata 1 orizzontale`. Lo schema delle caselle è lo stesso a ogni avvio perché, in riproduzione, il seme dipende dal brief.

## Cosa non provano

La riproduzione prova il flusso (generare, rivedere, accettare, scartare, controlli, cronometro), non la qualità dei testi: i testi sono inventati e fissi. La qualità dell'AI si vede solo nei passi dal vivo (compito `1b.2-dal-vivo`).

## Per rigenerare le Registrazioni

Strumento a mano, non una prova: `DBE_AGGIORNA_REGISTRAZIONI=1 dotnet test tests/DBEnigmistica.Tests -c Release --filter "Category=Strumento"`. Poi eliminare dalla cartella le Registrazioni obsolete (la prova `OgniBriefDellaSchermataSiRiproduceInUnCandidatoValidoSenzaControlli` fallisce finché mancano). I file in `brief/` sono i corpi che la schermata invia, senza codice né variante; la prova li invia a un host avviato come sopra.
