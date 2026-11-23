import { useEffect, useState } from 'react';
import { messaggioDiErrore } from '../../comune/api';
import type { Api } from '../../comune/api';
import { costruisciBrief, ETICHETTE_DIFFICOLTA, limitiDimensioni } from '../../comune/brief';
import type { Candidato, EsitoGenerazione, Tipologia } from '../../comune/tipi';
import './griglie.css';

interface Props {
  servizi: Pick<Api, 'tipologie' | 'genera'>;
  /** Facoltativo: la schermata mostra già da sé l'esito, con i candidati non validi e il messaggio. */
  dopoGenerazione?: (candidato: Candidato | null, messaggio: string | null) => void;
}

function intervallo(da: number, a: number): number[] {
  return Array.from({ length: a - da + 1 }, (_, i) => da + i);
}

/** Nuovo candidato per i giochi logici: solo difficoltà e dimensioni, senza tema né vincoli. */
export function NuovoCandidatoGriglie({ servizi, dopoGenerazione }: Props) {
  const [tipologie, setTipologie] = useState<Tipologia[] | null>(null);
  const [scelta, setScelta] = useState('');
  const [difficolta, setDifficolta] = useState(2);
  const [righe, setRighe] = useState(9);
  const [colonne, setColonne] = useState(9);
  const [inCorso, setInCorso] = useState(false);
  const [errore, setErrore] = useState<string | null>(null);
  const [esito, setEsito] = useState<EsitoGenerazione | null>(null);

  useEffect(() => {
    let attivo = true;
    servizi.tipologie().then(
      (elenco) => {
        if (!attivo) return;
        const logici = elenco.filter((t) => t.famiglia === 'logico');
        setTipologie(logici);
        const prima = logici[0];
        if (prima) imposta(prima);
      },
      (e: unknown) => attivo && setErrore(messaggioDiErrore(e)),
    );
    return () => {
      attivo = false;
    };
  }, [servizi]);

  function imposta(t: Tipologia) {
    const limiti = limitiDimensioni(t);
    setScelta(t.id);
    setRighe(limiti?.minimo ?? 9);
    setColonne(limiti?.minimo ?? 9);
  }

  const tipologia = tipologie?.find((t) => t.id === scelta) ?? null;
  const limiti = tipologia ? limitiDimensioni(tipologia) : null;

  async function genera() {
    if (!tipologia) return;
    setInCorso(true);
    setErrore(null);
    setEsito(null);
    try {
      const risposta = await servizi.genera(costruisciBrief(tipologia, { tema: '', sottotipo: '', vincoli: '', difficolta, righe, colonne }));
      setEsito(risposta);
      dopoGenerazione?.(risposta.candidati.find((c) => c.id === risposta.valido) ?? null, risposta.messaggio);
    } catch (e) {
      setErrore(messaggioDiErrore(e));
    } finally {
      setInCorso(false);
    }
  }

  if (tipologie === null) return errore ? <p role="alert">{errore}</p> : <p>Caricamento delle Tipologie…</p>;

  const nonValidi = esito?.candidati.filter((c) => c.stato === 'non_valido_schema' || c.stato === 'errore_generazione') ?? [];

  return (
    <form onSubmit={(e) => { e.preventDefault(); void genera(); }}>
      <label className="gr-campo">
        Gioco logico
        <select value={scelta} onChange={(e) => { const t = tipologie.find((x) => x.id === e.target.value); if (t) imposta(t); }}>
          {tipologie.map((t) => <option key={t.id} value={t.id}>{t.nome}</option>)}
        </select>
      </label>
      <label className="gr-campo">
        Difficoltà
        <select value={difficolta} onChange={(e) => setDifficolta(Number(e.target.value))}>
          {ETICHETTE_DIFFICOLTA.map((etichetta, i) => <option key={etichetta} value={i + 1}>{i + 1} - {etichetta}</option>)}
        </select>
      </label>
      {limiti?.lati && (
        <label className="gr-campo">
          Lato della griglia
          <select value={righe} onChange={(e) => { setRighe(Number(e.target.value)); setColonne(Number(e.target.value)); }}>
            {limiti.lati.map((n) => <option key={n} value={n}>{n}x{n}</option>)}
          </select>
        </label>
      )}
      {limiti && !limiti.lati && (
        <>
          <label className="gr-campo">
            Righe
            <select value={righe} onChange={(e) => setRighe(Number(e.target.value))}>
              {intervallo(limiti.minimo, limiti.massimo).map((n) => <option key={n} value={n}>{n}</option>)}
            </select>
          </label>
          <label className="gr-campo">
            Colonne
            <select value={colonne} onChange={(e) => setColonne(Number(e.target.value))}>
              {intervallo(limiti.minimo, limiti.massimo).map((n) => <option key={n} value={n}>{n}</option>)}
            </select>
          </label>
        </>
      )}
      <button type="submit" disabled={inCorso || tipologia === null || !tipologia.generazioneConsentita}>
        {inCorso ? 'Generazione in corso…' : 'Genera'}
      </button>
      {errore && <p role="alert">{errore}</p>}
      {esito?.messaggio && <p role="alert">{esito.messaggio}</p>}
      {nonValidi.length > 0 && (
        <section aria-label="Candidati non validi">
          <h4>Candidati non validi</h4>
          <ul>
            {nonValidi.map((c) => (
              <li key={c.id}>
                Candidato {c.ordineGenerazione} ({c.stato === 'non_valido_schema' ? 'non valido per lo schema' : 'errore di generazione'}): {c.messaggio ?? 'nessun dettaglio.'}
              </li>
            ))}
          </ul>
        </section>
      )}
    </form>
  );
}
