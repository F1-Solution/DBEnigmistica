# Decisioni dell'ondata 1b.2: date, passi dal vivo e prototipo con l'interruttore spento

Decisioni del proprietario del 23 novembre 2026 sulle quattro domande della sezione 6 di `.planning/phases/1b/preparazione-1b.2.md`. In tutti e quattro i casi il proprietario ha scelto la raccomandazione A. La 1b.2 si chiude il 7 dicembre 2026 senza chiamate AI dal vivo. Le chiamate dal vivo diventano un compito separato, `1b.2-dal-vivo`. Il prototipo si avvia sempre. Con l'interruttore dal vivo spento rifiuta solo le generazioni che userebbero l'AI.

## Termini

- **Interruttore dal vivo**: la chiave `FornitoreAi:DalVivoAbilitato`, che vale `false` per impostazione predefinita. Da evitare: «flag» e «toggle».
- **Passi dal vivo**: la generazione controllata con l'AI, una per famiglia, con l'interruttore dal vivo acceso. Sono possibili solo dopo la prova `1.1-prova` e con regole approvate.
- **Famiglie con AI**: crittografia breve, anagramma, rebus e cruciverba.

## Perché

La 1b.2 può chiudersi senza chiamate AI. Le chiamate dipendono da `1.1-prova` (condizioni d'uso entro il 6/12, prova entro il 13/12) e dalle regole approvate (le quattro v1 entro il 4/12, le v0 entro il 3/1/2027). Legare la chiusura a queste attese avrebbe spostato il collaudo a gennaio.

Con `DalVivoAbilitato = false` e senza Registrazioni in modalità riproduci, `dbe prototipo` non partiva affatto. Così il revisore non poteva provare nemmeno sudoku e giochi logici, che non usano l'AI.

## Decisioni

### D1b.2-1. Date della 1b.2

**Decisione.** La 1b.2 va dal 24/11 al 7/12/2026. La 1.3 parte l'8/12. Le scadenze esterne non cambiano: prototipo pronto verso il 10/1/2027, scadenza 17/1, prova 1 dal 18/1.

**Opzioni scartate.**
- *B: dal 24/11 al 14/12/2026, tre settimane.* Darebbe più margine alle schermate, ma consumerebbe il margine che serve ai passi dal vivo prima del 17/1.
- *C: date del piano, dal 21/12 al 3/1, con la 1.3 prima.* Spreca il tempo guadagnato con la chiusura della 1.2 il 23/11. La 1b.2 non ha bisogno di chiamate dal vivo.

### D1b.2-2. Passi dal vivo fuori dall'ondata

**Decisione.** L'ondata si chiude senza i passi dal vivo. Diventano il compito `1b.2-dal-vivo`, con la sua riga in `PROGRESS.md`: `PHASE:1b WAVE:1b.2-dal-vivo STATUS:complete`. Come `1.1-prova`, richiede `PHASE:1 WAVE:1.1-prova STATUS:complete` e le regole approvate.

**Opzioni scartate.**
- *B: l'ondata resta aperta finché non c'è una generazione dal vivo per famiglia.* Lega la chiusura del codice alle attese esterne, cioè le condizioni d'uso entro il 6/12 e le regole entro il 3/1/2027.
- *C: nessuna generazione controllata; i candidati della prova si generano subito.* Rischia di sprecare candidati della prova con un prompt difettoso.

### D1b.2-3. Prototipo con l'interruttore dal vivo spento

**Decisione.** `dbe prototipo` si avvia sempre. Con `FornitoreAi:DalVivoAbilitato=false`, solo *Genera* per crittografia, anagramma, rebus e cruciverba si rifiuta, con il messaggio di D8 e zero chiamate. Sudoku, futoshiki, nonogramma e la revisione funzionano. Questa decisione affina D8: l'ambito resta lo stesso, cioè nessuna chiamata dal vivo.

**Opzioni scartate.**
- *B: come oggi.* Il prototipo si usa prima di `1.1-prova` solo in modalità riproduci, e così si blocca la revisione anticipata di sudoku e giochi logici prevista dal ROADMAP.
- *C: un secondo interruttore solo per il prototipo.* Servirebbero due chiavi da tenere coerenti, con più configurazione.

### D1b.2-4. Collaudo e pre-release

**Decisione.** Il collaudo del proprietario (circa 1,5 ore) chiude l'ondata il 7/12/2026. Usa sudoku, futoshiki e nonogramma veri, e le famiglie con AI riprodotte dalle Registrazioni sintetiche. La pre-release `v0.2.0-prova.1` esce dalla CI subito dopo l'unione, senza spostare `latest`. Le correzioni fatte dopo i passi dal vivo escono come `v0.2.0-prova.2` (numerazione D1b-8).

**Opzioni scartate.**
- *B: collaudo e pre-release dopo i passi dal vivo, verso il 4-10/1/2027.* Il tempo del proprietario a gennaio serve per brief e regole.
- *C: due collaudi, uno breve il 7/12 senza pre-release e uno completo a gennaio.* Raddoppia il tempo del proprietario.

## Rischi

- **Famiglie con AI accettate su dati sintetici.** Il collaudo del 7/12 non vede generazioni vere. Il primo dato vero arriva con i passi dal vivo. Se serve una correzione, esce come `v0.2.0-prova.2`.
- **Passi dal vivo in ritardo.** Dipendono da `1.1-prova` (prova entro il 13/12) e dalle regole. Se slittano oltre il 17/1, i candidati della prova non sono pronti. Il margine nato dalle date di D1b.2-1 serve a questo.
- **Rifiuto senza chiamate da provare.** Con l'interruttore spento il rifiuto deve avvenire prima di ogni chiamata. Va coperto dai test della 1b.2.

## Rimandato e aperto

- Prova dello script locale (1.2, D6): entro il 27/11/2026. Non serve alla 1b.2.
- Le quattro regole v1 delle Tipologie (D3 della 1.2): approvazione entro il 4/12/2026.
- Condizioni d'uso dell'abbonamento: verifica entro il 6/12/2026.
- `1.1-prova`: prova entro il 13/12/2026.
- Regole v0 e brief corretti: entro il 3/1/2027.
- Tutte le regole dei nove descrittori sono ancora in bozza (`regole.stato = bozza`). Il compilatore del prompt le rifiuta fuori dai test, quindi nessuna generazione AI vera parte prima dell'approvazione.
