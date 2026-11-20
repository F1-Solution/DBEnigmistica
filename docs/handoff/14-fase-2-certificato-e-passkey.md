# 14 - Fase 2: certificato locale, passkey e collaudo dello Studio

**Stato:** Da fare
**Fase:** 2

> **Da dettagliare prima della fase 2.** I comandi e i nomi dei file arrivano con le ondate 2.1-2.3.

## Scopo

Installare il certificato locale dello Studio sui dispositivi di casa. Registrare le passkey. Fare il collaudo dello Studio in container sul PC Windows.

## Scadenza

Entro la chiusura della fase 2, stimata al **21 febbraio 2027**. Il collaudo cade durante la prova 1, quindi va tenuto breve (ROADMAP, fase 2).

## Tempo stimato

Circa 6 ore, totale della fase 2 (ROADMAP).

## Prerequisiti

- Lo Studio gira in container sul PC Windows, con Docker Desktop (decisione della ROADMAP).
- La decisione del file 15 sul container.

## Cosa ti serve

- Docker Desktop avviato sul PC.
- I dispositivi di casa da abilitare (PC, telefono, tablet).
- Un autenticatore compatibile con le passkey su ciascun dispositivo.

## Procedura (struttura prevista)

1. Installa il certificato radice locale su un dispositivo. Il certificato è prodotto dall'agente. Passo da dettagliare.
2. Ripeti il passo 1 per ogni dispositivo di casa.
3. Apri lo Studio dalla rete di casa. Registra una passkey. Passo da dettagliare.
4. Stampa il codice di recupero, se lo Studio lo offre. Conservalo fuori dal PC. Passo da dettagliare.
5. Esegui il collaudo di massimo un'ora. Passo da dettagliare dall'agente.

ATTENZIONE: Un dispositivo che non accetta il certificato usa lo Studio solo dal PC.

## Verifica finale

- Lo Studio si apre in HTTPS dalla rete di casa, su ogni dispositivo abilitato.
- Ogni dispositivo ha una passkey registrata.
- Il collaudo è terminato entro un'ora.

## Cosa consegnare all'agente principale

1. Elenco dei dispositivi abilitati, senza indirizzi IP.
2. Esito del collaudo e tempo impiegato.
3. Problemi incontrati.

## Cosa sblocca

- Fase 3: importazione e revisione.

---
Fonti: `ROADMAP.md`, fase 2 ("Obiettivo", "Riuso", "Dal proprietario (circa 6 h)", rischio "Accesso dai dispositivi di casa complicato").
