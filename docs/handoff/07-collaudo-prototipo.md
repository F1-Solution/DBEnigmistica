# 07 - Prova e collaudo del prototipo di creazione

**Stato:** In attesa (dipende dalle ondate 1b.1 e 1b.2)
**Fase:** 1b

## Scopo

Verificare che il prototipo funziona sul PC del proprietario, prima della prova 1. Il prototipo gira solo su `localhost`.

## Scadenza

**17 gennaio 2027**. Inizio della prova 1: 18 gennaio 2027.

## Tempo stimato

Circa 1,5 ore (ROADMAP, fase 1b).

## Prerequisiti

- File 06 completato: regole e brief.
- Eseguibile `dbe.exe` della pre-release `v0.2.0-prova.1`, pubblicato dalla CI (file 24). Il comando `dbe prototipo` arriva nella ondata 1b.1.
- File 00, Parte D: accesso a GHCR.
- Docker Desktop avviato, con PostgreSQL attivo (`docker compose up -d postgres`, file 00).

NOTA: Le schermate del prototipo sono "Nuovo candidato" e "Revisione". Il cronometro conta il tempo attivo. Le pause oltre 2 minuti non contano.

## Cosa ti serve

- PowerShell.
- Un browser.
- Il tag della pre-release, `v0.2.0-prova.1`, per scaricare `dbe.exe`.

## Procedura

1. Apri PowerShell nella cartella dove hai scaricato `dbe.exe`. Esegui il comando:

```powershell
.\dbe.exe prototipo
```

   Risultato: Il prototipo si avvia. Lascia la finestra aperta.

2. Apri il browser all'indirizzo `http://127.0.0.1:8080`.
   Risultato: Compare la schermata "Nuovo candidato".

3. Scegli la Tipologia `cruciverba-classico`. Inserisci un brief del file 06. Premi *Genera*.
   Risultato: Dopo l'attesa compare un candidato in "Revisione".

4. Modifica due campi. Premi *Accetta*.
   Risultato: Il candidato è accettato. Il cronometro si ferma.

5. Ripeti il passo 3 con un altro brief. Premi *Scarta*. Scrivi un motivo.
   Risultato: Il candidato è scartato con motivo.

6. Ripeti i passi 3-5 per ciascuna delle sei famiglie, con un candidato ciascuna.
   Risultato: Hai sei candidati di prova.

7. Annota ogni problema in un file di note: testo, schermata, passo.
   Risultato: Hai la lista degli attriti.

ATTENZIONE: Il prototipo non è un prodotto. Non pubblicare nulla da questo prototipo. I candidati accettati restano bozze.

## Verifica finale

- Le sei famiglie generano almeno un candidato.
- Modifica e accettazione funzionano.
- Lo scarto chiede un motivo.
- Il cronometro cambia durante la revisione.
- Il prototipo risponde solo su `127.0.0.1`. Controlla che un altro dispositivo della rete non lo raggiunga.

## Cosa consegnare all'agente principale

1. Esito di ogni verifica finale (sì o no).
2. La lista degli attriti, con schermata e passo.
3. Il tempo che hai impiegato per il collaudo.

## Cosa sblocca

- File 10: prova 1 a cronometro. Non parte senza collaudo positivo.

---
Fonti: `ROADMAP.md`, fase 1b, "Dal proprietario" (prova e collaudo del prototipo, circa 1,5 h), "Risultati consegnati" (schermate, avvio locale) e criteri di accettazione 1, 4 e 8; `ROADMAP.md`, sezione 1.3 (pre-release `v0.2.0-prova.1`); `docs/fase-1b-decisioni.md`, D1b-1; `docs/CI.md`, sezione "Creare una versione".
