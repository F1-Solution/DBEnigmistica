# 02 - Set di PDF di prova

**Stato:** Da fare
**Fase:** 1 (domanda aperta 14 della ROADMAP)

## Scopo

Fornire le Riviste in PDF che servono a costruire il Set di riferimento. Il Set di riferimento misura l'accuratezza del Motore di estrazione.

## Scadenza

7 dicembre 2026 (ROADMAP, domanda 14).

## Tempo stimato

Non stimato separatamente. È parte delle circa 20 ore della fase 1.

## Prerequisiti

- Le Riviste in PDF che possiedi.
- Il diritto d'uso privato delle Riviste. La domanda al legale è nel file 04.

AVVERTENZA: Non copiare i PDF nel repository `C:\Progetti\DBEnigmistica`. Non caricarli su GitHub, in chat o in un servizio online. Il Set di riferimento resta sul PC (ADR 008).

## Cosa ti serve

- Almeno 4 testate diverse.
- Almeno una scansione per testata. Il Set richiede almeno 5 pagine da scansioni.
- Una cartella sul PC fuori dal repository.

Composizione richiesta:

- 15-20 pagine con cruciverba e giochi brevi (anagramma, sciarada, cambio di lettera).
- Almeno 5 pagine da scansioni.
- Idealmente 20-30 pagine di tipologie diverse.

## Procedura

1. Apri PowerShell. Crea una cartella fuori dal repository. Esegui:

```powershell
New-Item -ItemType Directory -Path "C:\DBE-Set-di-riferimento"
```

   Risultato: La cartella esiste. Puoi usare un altro percorso, purché sia fuori dal repository.

2. Copia nella cartella i PDF scelti. Non cambiare il nome dei file.
   Risultato: La cartella contiene i PDF.

3. Crea il file `elenco.txt` nella stessa cartella. Per ogni PDF scrivi una riga con: nome del file, testata, numero, anno, e "digitale" oppure "scansione".
   Risultato: `elenco.txt` ha una riga per ogni PDF.

4. Apri ogni PDF. Controlla che si apra e che il numero di pagine sia quello atteso.
   Risultato: Nessun PDF è danneggiato.

5. Conta le pagine con cruciverba e giochi brevi. Conta quante sono scansioni.
   Risultato: `elenco.txt` riporta i due conteggi.

## Verifica finale

- La cartella esiste fuori dal repository.
- `elenco.txt` ha una riga per ogni PDF.
- Il conteggio delle pagine e delle scansioni è scritto.
- Il repository non contiene PDF. Controlla con:

```powershell
cd C:\Progetti\DBEnigmistica
git status --short
```

   Risultato: Nessun file PDF compare nell'elenco.

## Cosa consegnare all'agente principale

Scrivi un messaggio con:

1. Il percorso della cartella (per esempio `C:\DBE-Set-di-riferimento`).
2. Il numero di testate diverse.
3. Il conteggio delle pagine con cruciverba e giochi brevi, e quante sono scansioni.

Non inviare i PDF né il loro contenuto.

## Cosa sblocca

- File 08: correzione dei risultati attesi.
- Set di riferimento per `dbe eval` (ondata 1.2, disponibile dalla fase 1).
- Confronto dei Fornitori AI (ondata 1.4).

---
Fonti: `ROADMAP.md`, fase 1 ("Dal proprietario" e "Set di riferimento e misura di accuratezza") e sezione 7 (domanda 14); `docs/adr/008-set-di-riferimento-solo-sul-pc.md`.
