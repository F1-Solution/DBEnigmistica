import { useEffect, useMemo, useState } from 'react';
import { creaApi, messaggioDiErrore } from './comune/api';
import type { Api } from './comune/api';
import type { Tipologia } from './comune/tipi';
import { NuovoCandidato } from './schermate/NuovoCandidato';
import { Revisione } from './schermate/Revisione';

export type Percorso = { schermata: 'inizio' } | { schermata: 'nuovo'; tipologia?: string } | { schermata: 'revisione'; tipologia: string };

/** Percorsi nel frammento dell'indirizzo: #/nuovo[/tipologia] e #/revisione/tipologia. */
export function leggiPercorso(frammento: string): Percorso {
  const [, schermata, tipologia] = frammento.replace(/^#/, '').split('/');
  const id = tipologia ? decodeURIComponent(tipologia) : undefined;
  if (schermata === 'nuovo') return { schermata: 'nuovo', tipologia: id };
  if (schermata === 'revisione' && id) return { schermata: 'revisione', tipologia: id };
  return { schermata: 'inizio' };
}

function usePercorso(): Percorso {
  const [frammento, setFrammento] = useState(() => window.location.hash);
  useEffect(() => {
    const aggiorna = () => setFrammento(window.location.hash);
    window.addEventListener('hashchange', aggiorna);
    return () => window.removeEventListener('hashchange', aggiorna);
  }, []);
  return useMemo(() => leggiPercorso(frammento), [frammento]);
}

interface AppProps {
  api?: Api;
}

export function App({ api: apiDaUsare }: AppProps) {
  const api = useMemo(() => apiDaUsare ?? creaApi(), [apiDaUsare]);
  const percorso = usePercorso();

  return (
    <>
      <header className="intestazione">
        <h1>DBEnigmistica - Prototipo di revisione</h1>
        <nav aria-label="Principale">
          <a href="#/">Inizio</a> <a href="#/nuovo">Nuovo candidato</a>
        </nav>
      </header>
      <main>
        {percorso.schermata === 'inizio' ? <Inizio api={api} /> : null}
        {percorso.schermata === 'nuovo' ? <NuovoCandidato api={api} tipologiaIniziale={percorso.tipologia} /> : null}
        {percorso.schermata === 'revisione' ? <Revisione key={percorso.tipologia} tipologia={percorso.tipologia} api={api} /> : null}
      </main>
    </>
  );
}

function Inizio({ api }: { api: Api }) {
  const [tipologie, setTipologie] = useState<Tipologia[] | null>(null);
  const [errore, setErrore] = useState<string | null>(null);

  useEffect(() => {
    let attivo = true;
    api.tipologie().then(
      (elenco) => attivo && setTipologie(elenco),
      (e: unknown) => attivo && setErrore(messaggioDiErrore(e)),
    );
    return () => {
      attivo = false;
    };
  }, [api]);

  if (errore)
    return (
      <p role="alert" className="errore">
        {errore}
      </p>
    );
  if (!tipologie) return <p>Caricamento delle Tipologie…</p>;

  return (
    <section aria-labelledby="titolo-tipologie">
      <h2 id="titolo-tipologie">Tipologie</h2>
      <ul className="tipologie">
        {tipologie.map((t) => (
          <li key={t.id}>
            <strong>{t.nome}</strong> ({t.modo === 'ai' ? 'AI' : 'algoritmo'}
            {t.modo === 'ai' && !t.regoleApprovate ? ', regole in bozza' : ''}){' '}
            <a href={`#/nuovo/${encodeURIComponent(t.id)}`}>Nuovo candidato</a> <a href={`#/revisione/${encodeURIComponent(t.id)}`}>Rivedi</a>
          </li>
        ))}
      </ul>
    </section>
  );
}
