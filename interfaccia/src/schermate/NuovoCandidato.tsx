import { useEffect, useId, useState } from 'react';
import { messaggioDiErrore } from '../comune/api';
import type { Api } from '../comune/api';
import { costruisciBrief, ETICHETTE_DIFFICOLTA, limitiDimensioni } from '../comune/brief';
import type { ValoriBrief } from '../comune/brief';
import type { EsitoGenerazione, Tipologia } from '../comune/tipi';
import { NuovoCandidatoGriglie } from './griglie/NuovoCandidatoGriglie';

/** Voce del selettore che apre il modulo dei giochi logici (sudoku, futoshiki, nonogramma). */
const GIOCHI_LOGICI = '__logici__';

const MESSAGGIO_REGOLE_IN_BOZZA =
  "Regole in bozza: la generazione con l'AI non è disponibile finché il proprietario non approva le regole di questa Tipologia.";

interface NuovoCandidatoProps {
  api: Pick<Api, 'tipologie' | 'genera'>;
  /** Tipologia proposta all'apertura (per esempio quella appena rivista). */
  tipologiaIniziale?: string;
}

const VALORI_INIZIALI: ValoriBrief = { tema: '', sottotipo: '', vincoli: '', difficolta: 3, righe: 9, colonne: 9 };

/** Valori di partenza del modulo: le dimensioni cominciano dal minimo ammesso. */
function valoriPer(tipologia: Tipologia | undefined): ValoriBrief {
  const limiti = tipologia ? limitiDimensioni(tipologia) : null;
  return { ...VALORI_INIZIALI, righe: limiti?.minimo ?? 9, colonne: limiti?.minimo ?? 9 };
}

export function NuovoCandidato({ api, tipologiaIniziale }: NuovoCandidatoProps) {
  const [tipologie, setTipologie] = useState<Tipologia[] | null>(null);
  const [scelta, setScelta] = useState(tipologiaIniziale ?? '');
  const [valori, setValori] = useState<ValoriBrief>(VALORI_INIZIALI);
  const [errore, setErrore] = useState<string | null>(null);
  const [esito, setEsito] = useState<EsitoGenerazione | null>(null);
  const [inCorso, setInCorso] = useState(false);
  const idTipologia = useId();

  useEffect(() => {
    let attivo = true;
    api.tipologie().then(
      (elenco) => {
        if (!attivo) return;
        const iniziale = elenco.find((t) => t.id === tipologiaIniziale);
        const id = iniziale ? (iniziale.famiglia === 'logico' ? GIOCHI_LOGICI : iniziale.id) : (elenco.find((t) => t.famiglia !== 'logico')?.id ?? GIOCHI_LOGICI);
        setTipologie(elenco);
        setScelta(id);
        setValori(valoriPer(elenco.find((t) => t.id === id)));
      },
      (e: unknown) => attivo && setErrore(messaggioDiErrore(e)),
    );
    return () => {
      attivo = false;
    };
  }, [api, tipologiaIniziale]);

  const tipologia = tipologie?.find((t) => t.id === scelta) ?? null;

  function cambiaTipologia(id: string) {
    setScelta(id);
    setEsito(null);
    setErrore(null);
    setValori(valoriPer(tipologie?.find((t) => t.id === id)));
  }

  const cambia = <K extends keyof ValoriBrief>(nome: K, valore: ValoriBrief[K]) => setValori((v) => ({ ...v, [nome]: valore }));

  async function genera() {
    if (!tipologia) return;
    setInCorso(true);
    setErrore(null);
    setEsito(null);
    try {
      setEsito(await api.genera(costruisciBrief(tipologia, valori)));
    } catch (e) {
      setErrore(messaggioDiErrore(e));
    } finally {
      setInCorso(false);
    }
  }

  if (tipologie === null) {
    return errore ? (
      <p role="alert" className="errore">
        {errore}
      </p>
    ) : (
      <p>Caricamento delle Tipologie…</p>
    );
  }

  const logici = scelta === GIOCHI_LOGICI;
  const bloccata = tipologia !== null && !tipologia.generazioneConsentita;
  const inBozza = tipologia !== null && tipologia.modo === 'ai' && tipologia.statoRegole === 'bozza';
  // Un host più vecchio non manda il motivo: per le regole in bozza vale il testo di riserva.
  const motivo = tipologia?.motivoGenerazioneNonConsentita ?? (inBozza ? MESSAGGIO_REGOLE_IN_BOZZA : null);

  return (
    <div className="nuovo-candidato">
      <h2>Nuovo candidato</h2>
      <div className="campo">
        <label htmlFor={idTipologia}>Tipologia</label>
        <select id={idTipologia} value={scelta} onChange={(e) => cambiaTipologia(e.target.value)}>
          {tipologie
            .filter((t) => t.famiglia !== 'logico')
            .map((t) => (
              <option key={t.id} value={t.id}>
                {t.nome}
              </option>
            ))}
          {tipologie.some((t) => t.famiglia === 'logico') ? <option value={GIOCHI_LOGICI}>Giochi logici</option> : null}
        </select>
      </div>

      {logici ? (
        <NuovoCandidatoGriglie servizi={api} />
      ) : null}

      {tipologia ? (
        <>
          {inBozza && !bloccata ? (
            <p role="note" className="avviso">
              Regole in bozza: la generazione usa le Registrazioni in modalità riproduci, senza l&apos;AI dal vivo.
            </p>
          ) : null}
          <CampiBrief tipologia={tipologia} valori={valori} cambia={cambia} />
          <button type="button" className="primario" disabled={inCorso || bloccata} onClick={() => void genera()}>
            {inCorso ? 'Generazione in corso…' : 'Genera'}
          </button>
          {bloccata && motivo ? (
            <p role="note" className="avviso">
              {motivo}
            </p>
          ) : null}
        </>
      ) : null}

      {errore ? (
        <p role="alert" className="errore">
          {errore}
        </p>
      ) : null}
      {esito ? <EsitoDellaGenerazione esito={esito} tipologia={scelta} /> : null}
    </div>
  );
}

interface CampiBriefProps {
  tipologia: Tipologia;
  valori: ValoriBrief;
  cambia: <K extends keyof ValoriBrief>(nome: K, valore: ValoriBrief[K]) => void;
}

function CampiBrief({ tipologia, valori, cambia }: CampiBriefProps) {
  const id = useId();
  const ha = (nome: string) => tipologia.campiBrief.find((c) => c.nome === nome);
  const tema = ha('tema');
  const sottotipo = ha('sottotipo');
  const vincoli = ha('vincoli');
  const limiti = limitiDimensioni(tipologia);

  return (
    <>
      {tema ? (
        <div className="campo">
          <label htmlFor={`${id}-tema`}>{tema.obbligatorio ? 'Tema' : 'Tema (facoltativo: vuoto = libero)'}</label>
          <input id={`${id}-tema`} type="text" maxLength={tema.massimo ?? 200} required={tema.obbligatorio} value={valori.tema} onChange={(e) => cambia('tema', e.target.value)} />
        </div>
      ) : null}
      {sottotipo ? (
        <div className="campo">
          <label htmlFor={`${id}-sottotipo`}>Sottotipo</label>
          <select id={`${id}-sottotipo`} value={valori.sottotipo} onChange={(e) => cambia('sottotipo', e.target.value)}>
            <option value="">Scegli un sottotipo</option>
            {sottotipo.valori.map((v) => (
              <option key={v} value={v}>
                {v}
              </option>
            ))}
          </select>
        </div>
      ) : null}
      {vincoli ? (
        <div className="campo">
          <label htmlFor={`${id}-vincoli`}>Vincoli (uno per riga, al massimo {vincoli.massimo ?? 8})</label>
          <textarea id={`${id}-vincoli`} rows={3} value={valori.vincoli} onChange={(e) => cambia('vincoli', e.target.value)} />
        </div>
      ) : null}
      <div className="campo">
        <label htmlFor={`${id}-difficolta`}>Difficoltà</label>
        <select id={`${id}-difficolta`} value={valori.difficolta} onChange={(e) => cambia('difficolta', Number(e.target.value))}>
          {ETICHETTE_DIFFICOLTA.map((etichetta, i) => (
            <option key={etichetta} value={i + 1}>
              {i + 1} - {etichetta}
            </option>
          ))}
        </select>
      </div>
      {limiti ? <CampoDimensioni id={id} limiti={limiti} valori={valori} cambia={cambia} /> : null}
    </>
  );
}

interface CampoDimensioniProps extends CampiBriefProps2 {
  id: string;
  limiti: NonNullable<ReturnType<typeof limitiDimensioni>>;
}
type CampiBriefProps2 = Pick<CampiBriefProps, 'valori' | 'cambia'>;

function CampoDimensioni({ id, limiti, valori, cambia }: CampoDimensioniProps) {
  if (limiti.lati) {
    return (
      <div className="campo">
        <label htmlFor={`${id}-lato`}>Dimensioni (quadrata)</label>
        <select
          id={`${id}-lato`}
          value={valori.righe}
          onChange={(e) => {
            cambia('righe', Number(e.target.value));
            cambia('colonne', Number(e.target.value));
          }}
        >
          {limiti.lati.map((lato) => (
            <option key={lato} value={lato}>
              {lato} x {lato}
            </option>
          ))}
        </select>
      </div>
    );
  }

  return (
    <div className="campo-doppio">
      <div className="campo">
        <label htmlFor={`${id}-righe`}>Righe</label>
        <input id={`${id}-righe`} type="number" min={limiti.minimo} max={limiti.massimo} value={valori.righe} onChange={(e) => cambia('righe', Number(e.target.value))} />
      </div>
      <div className="campo">
        <label htmlFor={`${id}-colonne`}>Colonne</label>
        <input id={`${id}-colonne`} type="number" min={limiti.minimo} max={limiti.massimo} value={valori.colonne} onChange={(e) => cambia('colonne', Number(e.target.value))} />
      </div>
    </div>
  );
}

function EsitoDellaGenerazione({ esito, tipologia }: { esito: EsitoGenerazione; tipologia: string }) {
  const nonValidi = esito.candidati.filter((c) => c.stato === 'non_valido_schema' || c.stato === 'errore_generazione');
  return (
    <section aria-labelledby="titolo-esito-generazione" className="esito-generazione">
      <h3 id="titolo-esito-generazione">Risultato della generazione</h3>
      {esito.valido ? (
        <p role="status">
          Candidato generato. <a href={`#/revisione/${encodeURIComponent(esito.candidati.at(-1)?.tipologia ?? tipologia)}`}>Vai alla revisione</a>
        </p>
      ) : null}
      {esito.messaggio ? (
        <p role="alert" className="errore">
          {esito.messaggio}
        </p>
      ) : null}
      {nonValidi.length > 0 ? (
        <>
          <h4>Candidati non validi</h4>
          <ul>
            {nonValidi.map((c) => (
              <li key={c.id}>
                Candidato {c.ordineGenerazione} ({c.stato === 'non_valido_schema' ? 'non valido per lo schema' : 'errore di generazione'}):{' '}
                {c.messaggio ?? 'nessun dettaglio.'}
              </li>
            ))}
          </ul>
        </>
      ) : null}
    </section>
  );
}
