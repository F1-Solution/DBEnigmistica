# ADR 007: host web del prototipo e ossatura React

- **Stato:** Proposto (in attesa di accettazione del proprietario)
- **Data:** 23/11/2026
- **Integra:** ADR 006 (stack e riga di comando). Non sostituisce nessun ADR precedente.
- **Fonte:** `docs/fase-1b-decisioni.md`, decisione D1b-9; `docs/ondata-1b-2-decisioni.md`

## Contesto

Il prototipo di revisione (`dbe prototipo`) mostra i candidati in un browser sul PC del proprietario. La fase 2 prevede un'interfaccia React e TypeScript con API ASP.NET Core, ma il codice che la fase 2 eredita nasce già nella fase 1b. L'ADR deve esistere quando nasce il codice, quindi si scrive nella 1b.2. Le scelte qui sono già del proprietario: questo documento le registra.

Restano fuori da questo ADR il client generato dallo schema OpenAPI, l'involucro desktop e il design system: sono della fase 2.

## Decisione

1. **Host.** L'host del prototipo è ASP.NET Core in ascolto solo sull'indirizzo di loopback `127.0.0.1`, porta predefinita 5180. L'indirizzo è fisso nel codice e non si cambia da configurazione.
2. **Filtro locale.** Ogni richiesta deve avere `Host` uguale a `127.0.0.1` o `localhost`. Se dichiara un'origine, questa deve coincidere con l'indirizzo dell'host. Una richiesta con `Sec-Fetch-Site: cross-site` è rifiutata. Le richieste con corpo devono essere `application/json`.
3. **Interfaccia servita dall'host.** L'host serve i file statici dalla cartella `wwwroot` della propria pubblicazione. Una richiesta GET fuori da `/api`, senza estensione, riceve `index.html`, così il percorso a frammento funziona al ricaricamento. Le rotte `/api/prototipo` non cambiano.
4. **Intestazioni di sicurezza su ogni risposta.** `Content-Security-Policy: default-src 'self'; base-uri 'none'; form-action 'self'; object-src 'none'; frame-ancestors 'none'`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer`. Le pagine non caricano nulla da domini esterni.
5. **Interfaccia nel repository.** La cartella `interfaccia/` nella radice ha un proprio `package.json` e il proprio lockfile. Il build (`npm run build`) scrive in `interfaccia/dist/`. La build .NET copia `interfaccia/dist/` in `wwwroot/` dell'output e della pubblicazione, senza pacchetti aggiuntivi e senza file incorporati.
6. **Interfaccia obbligatoria in CI e pubblicazione.** Con la proprietà `RichiediInterfaccia=true` la compilazione fallisce se manca `interfaccia/dist/index.html`. Senza la proprietà, l'host risponde 503 con la richiesta di eseguire `npm run build`.
7. **Ossatura React.** React 19 e TypeScript con Vite. Le schermate sono in `interfaccia/src/schermate/` (`parole/`, `griglie/`, `rebus/`), i componenti e il client dell'API in `interfaccia/src/comune/`. Un mapa da Tipologia all'editor sta in `interfaccia/src/schermate/`.
8. **Instradamento a frammento.** Le schermate si scelgono dal frammento dell'indirizzo (`#/`, `#/nuovo/<tipologia>`, `#/revisione/<tipologia>`), senza libreria di routing.
9. **Nessuna libreria di componenti.** Gli stili stanno in un solo foglio, sotto `interfaccia/src/`, con le variabili CSS.
10. **Client dell'API scritto a mano.** Il client è tipizzato e sta in `interfaccia/src/comune/`. Il client generato dallo schema è rimandato alla fase 2.
11. **Versioni fissate.** Le versioni dei pacchetti npm sono esatte, senza `^` né `~`: React e React DOM 19.1.10, Vite 8.3.6, Vitest 5.1.0, jsdom 31.0.0, TypeScript 6.0.3, ESLint 10.14.0, typescript-eslint 8.72.1, Testing Library (React 16.3.3, jest-dom 7.0.1, user-event 14.6.8). Pacchetti di sviluppo: @eslint/js 10.0.1, eslint-plugin-react-hooks 7.1.1, globals 17.14.0, @types/node 24.10.1, @types/react 19.3.0, @types/react-dom 19.3.0, @vitejs/plugin-react 6.2.1. Node 24 LTS. TypeScript 7 non si usa: typescript-eslint 8.72.1 accetta solo TypeScript minore di 6.1.
12. **Test end-to-end.** Playwright per .NET, solo Chromium, nella suite .NET. Non si usa axe: l'accessibilità del Sito non si applica al prototipo.
13. **Ambiente di sviluppo.** Il server di sviluppo di Vite inoltra `/api` all'host sulla porta 5180 e toglie l'intestazione `Origin`. Così il filtro locale dell'host resta invariato.

## Alternative scartate

- **Unificare subito i due lettori dei descrittori delle Tipologie.** Il rischio di toccare il caricamento a poche settimane dalla prova è più alto del beneficio. Passa alla fase 2 (D1b-9, R1 della preparazione 1b.2).
- **Incorporare i file dell'interfaccia nell'eseguibile.** Aggiunge un passo di generazione e non è necessario per un host locale.
- **Client generato dallo schema OpenAPI.** Anticipa lavoro della fase 2.

## Conseguenze

- Il prototipo richiede Node 24 per costruire l'interfaccia. L'archivio di pubblicazione contiene l'interfaccia già costruita.
- Il filtro locale resta la sola difesa contro richieste da altri siti: le intestazioni di sicurezza non la sostituiscono.
- **Licenze.** Gli strumenti di sviluppo e build possono usare MPL-2.0, BlueOak-1.0.0 e CC-BY-4.0 come eccezione (D1b.2-5 in `docs/ondata-1b-2-decisioni.md`); le dipendenze servite nelle pagine restano permissive.
- **Per la fase 2.** I colori e le spaziature del foglio di stile sono variabili CSS: la fase 2 le sostituisce con i token di identità. Il lavoro React della fase 2 si sposta in un workspace accanto a `packages/lettore`, senza spostare `interfaccia/`.
- **Per la fase 2.** Questo ADR si sostituisce con un ADR nuovo quando la fase 2 estende l'interfaccia. Il presente non si riscrive.
