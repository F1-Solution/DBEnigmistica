import { useEffect, useState } from 'react';
import { messaggioDiErrore } from '../comune/api';
import type { Api } from '../comune/api';
import { ContestoRegistro, creaRegistro } from '../comune/registroSalvataggio';
import { EsitoRevisione } from '../comune/EsitoRevisione';
import type { Esito } from '../comune/EsitoRevisione';
import { formattaTempo, useCronometro } from '../comune/useCronometro';
import type { Candidato } from '../comune/tipi';
import { EDITORI } from './editori';

interface RevisioneProps {
  tipologia: string;
  api: Api;
}

type Fase = { tipo: 'carico' } | { tipo: 'vuoto' } | { tipo: 'errore'; messaggio: string } | { tipo: 'candidato'; candidato: Candidato };

/** Il prossimo candidato della Tipologia, in ordine di generazione: rivedi, modifica, accetta o scarta. */
export function Revisione({ tipologia, api }: RevisioneProps) {
  const [fase, setFase] = useState<Fase>({ tipo: 'carico' });
  const [ultimoEsito, setUltimoEsito] = useState<string | null>(null);

  const [giro, setGiro] = useState(0);

  useEffect(() => {
    let attivo = true;
    api.prossimo(tipologia).then(
      (candidato) => attivo && setFase(candidato ? { tipo: 'candidato', candidato } : { tipo: 'vuoto' }),
      (e: unknown) => attivo && setFase({ tipo: 'errore', messaggio: messaggioDiErrore(e) }),
    );
    return () => {
      attivo = false;
    };
  }, [api, tipologia, giro]);

  const ricarica = () => {
    setFase({ tipo: 'carico' });
    setGiro((g) => g + 1);
  };

  const alla = (esito: Esito) => {
    setUltimoEsito(esito === 'accettato' ? 'Candidato accettato.' : 'Candidato scartato.');
    ricarica();
  };

  return (
    <div className="revisione">
      <h2>Revisione: {tipologia}</h2>
      {ultimoEsito ? (
        <p role="status" className="esito-precedente">
          {ultimoEsito}
        </p>
      ) : null}
      {fase.tipo === 'carico' ? <p>Caricamento del prossimo candidato…</p> : null}
      {fase.tipo === 'vuoto' ? (
        <p>
          Nessun candidato da rivedere. <a href={`#/nuovo/${encodeURIComponent(tipologia)}`}>Genera un nuovo candidato</a>
        </p>
      ) : null}
      {fase.tipo === 'errore' ? (
        <div>
          <p role="alert" className="errore">
            {fase.messaggio}
          </p>
          <button type="button" className="secondario" onClick={ricarica}>
            Riprova
          </button>
        </div>
      ) : null}
      {fase.tipo === 'candidato' ? <CandidatoInRevisione key={fase.candidato.id} candidato={fase.candidato} api={api} onConcluso={alla} /> : null}
    </div>
  );
}

interface CandidatoInRevisioneProps {
  candidato: Candidato;
  api: Api;
  onConcluso: (esito: Esito) => void;
}

function CandidatoInRevisione({ candidato, api, onConcluso }: CandidatoInRevisioneProps) {
  const { cronometro, tempoAttivoMs } = useCronometro(candidato, api);
  const [registro] = useState(creaRegistro);
  const Editor = EDITORI[candidato.tipologia];

  const pie = (
    <>
      <EsitoRevisione candidato={candidato} servizi={api} cronometro={cronometro} onConcluso={onConcluso} />
    </>
  );

  return (
    <article aria-label={`Candidato ${candidato.ordineGenerazione}`}>
      <header className="testata-candidato">
        <p>
          Candidato {candidato.ordineGenerazione} ({candidato.modo === 'ai' ? 'AI' : 'algoritmo'})
        </p>
        <p className="cronometro" role="timer" aria-label="Tempo attivo di revisione">
          Tempo attivo {formattaTempo(tempoAttivoMs)}
        </p>
      </header>
      {Editor ? (
        <ContestoRegistro.Provider value={registro}>
          <Editor candidato={candidato} servizi={api}>
            {pie}
          </Editor>
        </ContestoRegistro.Provider>
      ) : (
        <p role="alert" className="errore">
          Nessun editor per la Tipologia {candidato.tipologia}.
        </p>
      )}
    </article>
  );
}
