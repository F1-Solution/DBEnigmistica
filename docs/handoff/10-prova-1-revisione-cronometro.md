# 10 - Prova 1: revisione a cronometro dei candidati

**Stato:** In attesa (dipende dai file 06 e 07)
**Fase:** 1b

## Scopo

Revisionare 5 candidati per ciascuna delle sei famiglie, con il cronometro. La prova misura il tempo di revisione e la quota accettata. Il risultato decide la soglia G1 (file 11).

## Scadenza

- **Inizio:** 18 gennaio 2027.
- **Fine:** 21 febbraio 2027.

## Tempo stimato

Circa 14 ore in totale. La ROADMAP stima:

| Famiglia | Tempo per il G1 | Candidati |
|---|---|---|
| Crittografia breve | circa 100 minuti | 5 |
| Anagramma a tema | circa 100 minuti | 5 |
| Cruciverba classico | circa 300 minuti | 5 |
| Rebus | 150-225 minuti (30-45 minuti l'uno) | 5 |
| Sudoku e giochi logici | circa 50 minuti | 5 per famiglia |

A questi si aggiunge circa un'ora per brief, note e pause.

## Prerequisiti

- File 07 completato: collaudo positivo.
- File 06 completato: brief e regole.
- Il prototipo gira sul PC (file 07, passo 1).

ATTENZIONE: La quota dell'abbonamento è condivisa con lo sviluppo. Genera i candidati a parole fuori dagli orari di sviluppo, in anticipo. La sessione di revisione non aspetta il Fornitore AI.

AVVERTENZA: Nessun candidato va pubblicato. I candidati accettati restano bozze.

## Cosa ti serve

- Il prototipo in esecuzione su `http://127.0.0.1:8080`.
- I brief del file 06.
- Un file di note per gli attriti.

## Procedura

1. Scegli una sessione di almeno 45 minuti, fuori dagli orari di sviluppo.
   Risultato: Hai una sessione pianificata.

2. Avvia il prototipo. Apri `http://127.0.0.1:8080`.
   Risultato: Compare la schermata "Nuovo candidato".

3. Per una famiglia, scegli la Tipologia. Inserisci tema, vincoli e difficoltà dal brief. Premi *Genera*.
   Risultato: Compare un candidato in "Revisione".

4. Controlla il candidato. Modifica i campi che non vanno, uno per uno.
   Risultato: Ogni modifica è salvata.

5. Premi *Accetta* se il candidato è utilizzabile. Premi *Scarta* se non lo è. Con *Scarta*, scrivi il motivo.
   Risultato: L'esito è registrato.

6. Ripeti i passi 3-5 finché la famiglia ha cinque candidati.
   Risultato: La famiglia ha cinque esiti.

7. Passa alla famiglia successiva. Ripeti i passi 3-6 fino a sei famiglie.
   Risultato: Hai trenta esiti in totale.

8. Annota ogni attrito. Scrivi la famiglia, il passo e il tempo perso.
   Risultato: Hai la lista degli attriti.

9. Invia all'agente principale il rapporto `dbe eval review-time`. Il comando è disponibile dalla fase 1b.
   Risultato: L'agente ha il rapporto.

NOTA: Per sudoku e giochi logici il rapporto dà i tempi, senza soglia G1. Il rebus usa descrizioni a parole delle vignette, non immagini.

## Verifica finale

- Trenta candidati hanno un esito, accettato o scartato.
- Gli scarti hanno un motivo.
- Il rapporto `dbe eval review-time` è stato prodotto.

## Cosa consegnare all'agente principale

1. Il rapporto `dbe eval review-time`, in formato JSON e in formato leggibile.
2. La lista degli attriti.
3. Il numero di ore effettive impiegate.

## Cosa sblocca

- File 11: decisione G1.
- Correzioni della ondata 1b.3 (attriti della prima settimana).

---
Fonti: `ROADMAP.md`, fase 1b, "Obiettivo", "Dal proprietario" (prova 1, circa 14 h, tempi per famiglia), "Criteri di accettazione" 5 e 6, "Rischi" (contesa dei limiti dell'abbonamento); `ROADMAP.md`, sezione 5.3 (soglie G1).
