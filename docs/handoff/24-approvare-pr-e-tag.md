# 24 - Approvare una pull request, unirla e creare un tag di versione

**Stato:** Ricorrente (si ripete a fine di ogni ondata e di ogni fase)
**Fase:** tutte. Prima occorrenza: pre-release del prototipo, prima del 18 gennaio 2027.

## Scopo

Approvare la fusione di una ondata in `main`. Creare un tag di versione, che fa partire la pubblicazione dell'immagine e dell'eseguibile. Chiudere una fase solo con la tua approvazione.

## Scadenza

- Ogni ondata: alla fine dell'ondata.
- Pre-release `v0.2.0-prova.1`: prima della prova 1, quindi prima del 18 gennaio 2027.
- Pre-release `v0.2.0-prova.2`, `-prova.3` e così via: correzioni della 1b.3, prima di `v0.2.0`, senza spostare `latest`.
- Release `v0.2.0`: fine della fase 1, entro il 21 febbraio 2027.

## Tempo stimato

Circa 15 minuti per ogni PR. Il tempo per le release non è stimato nella ROADMAP.

## Prerequisiti

- Una pull request aperta dall'agente verso `main`.
- Per il tag: `main` aggiornato e con la CI verde.

AVVERTENZA: La protezione di `main` è rimandata (decisione D1). GitHub non blocca un merge con la CI rossa. Controlla tu la CI prima di unire.

## Cosa ti serve

- Browser con l'account GitHub del repository.
- PowerShell, con Git installato.

## Procedura

### Parte A - Approvare e unire una pull request

1. Apri il repository su GitHub. Vai su *Pull requests*. Apri la PR indicata dall'agente.
   Risultato: Vedi la descrizione e i file cambiati.

2. Apri la scheda *Checks*, o la sezione dei controlli in fondo alla PR.
   Risultato: Vedi i lavori della CI. Tutti devono essere verdi. I lavori sono *Build e test*, *Vulnerabilita pacchetti*, *Controllo segreti* e *Build immagine Docker*.

   ATTENZIONE: Se un lavoro è rosso, non unire. Avvisa l'agente.

3. Leggi la descrizione della PR. Controlla che corrisponda alla ondata indicata.
   Risultato: La PR corrisponde alla ondata.

4. Scegli *Approve* se la PR è approvata. Poi scegli *Merge pull request*.
   Risultato: La PR è unita in `main`.

5. Conferma l'approvazione all'agente con una riga.
   Risultato: L'agente scrive la riga in `PROGRESS.md`.

### Parte B - Creare un tag di versione

Il tag fa partire la pubblicazione (`publish.yml`). Il tag deve puntare a un commit già unito in `main`.

6. Apri PowerShell nella cartella del repository. Passa a `main` e aggiornalo:

```powershell
cd C:\Progetti\DBEnigmistica
git checkout main
git pull
```

   Risultato: `main` è aggiornato.

7. Controlla che la CI sia verde sull'ultimo commit di `main`. Usa la pagina *Actions* su GitHub.
   Risultato: L'ultimo commit ha la CI verde.

8. Crea il tag. Sostituisci `vX.Y.Z` con la versione indicata dall'agente. Per una pre-release usa il suffisso, per esempio `v0.2.0-prova.1`:

```powershell
git tag -a vX.Y.Z -m "Versione X.Y.Z"
git push origin vX.Y.Z
```

   Risultato: Il tag è inviato. La pubblicazione parte su *Actions > Publish*.

   NOTA: Un tag con suffisso crea una pre-release. L'immagine non riceve il tag `latest`.

9. Attendi la fine del flusso *Publish* su *Actions*.
   Risultato: Tutti i lavori di *Publish* sono verdi.

   Se il flusso si ferma, il tag è su un commit fuori da `main`, o la CI è rossa. Non riprovare. Avvisa l'agente.

### Parte C - Verificare l'immagine

10. Accedi a GHCR (file 00, Parte D), se non l'hai già fatto. Esegui, sostituendo `X.Y.Z`:

```powershell
docker run --rm ghcr.io/f1-solution/dbenigmistica:X.Y.Z --version
```

    Risultato: Il comando stampa la versione del tag.

### Parte D - Annullare un tag sbagliato

Il tag va annullato solo prima che la pubblicazione finisca.

11. Elimina il tag su GitHub. Esegui:

```powershell
git push origin :refs/tags/vX.Y.Z
```

    Risultato: Il tag è rimosso dal repository remoto.

12. Se la release esiste, eliminala da *Releases*.
    Risultato: La release non compare più.

## Verifica finale

- La PR è unita in `main` con la CI verde.
- Il tag è creato e il flusso *Publish* è verde.
- Il comando `--version` stampa la versione del tag.

## Cosa consegnare all'agente principale

1. Numero della PR unita, e data.
2. Tag creato, oppure "nessun tag".
3. Esito del flusso *Publish*: verde o rosso.
4. Esito del comando `--version`.

## Cosa sblocca

- La prova 1 (file 10), con la pre-release `v0.2.0-prova.1`.
- La chiusura della fase. Senza la tua approvazione scritta, la fase non si chiude (ROADMAP, sezione 1.2).

---
Fonti: `docs/CI.md`, sezioni "Flussi" (Publish), "Creare una versione" e "Impostazioni da fare a mano su GitHub"; `docs/fase-0-chiusura-decisioni.md` (D1, ordine di chiusura, passo 4); `ROADMAP.md`, sezioni 1.2 (collaudo e chiusura) e 1.3; `.planning/phases/1/CONTEXT.md` (approvazione di PR e tag).
