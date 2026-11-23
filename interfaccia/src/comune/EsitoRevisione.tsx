import { useContext, useId, useState, useSyncExternalStore } from 'react';
import { messaggioDiErrore } from './api';
import type { Api } from './api';
import type { Cronometro } from './cronometro';
import { ContestoRegistro } from './registroSalvataggio';
import type { StatoRegistro } from './registroSalvataggio';
import { MASSIMO_NOTA, MOTIVI_SCARTO } from './tipi';
import type { Candidato } from './tipi';

export type Esito = 'accettato' | 'scartato';

interface EsitoRevisioneProps {
  candidato: Pick<Candidato, 'id'>;
  servizi: Pick<Api, 'accetta' | 'scarta'>;
  cronometro: Pick<Cronometro, 'concludi' | 'ferma'>;
  onConcluso: (esito: Esito) => void;
}

const NESSUN_SALVATAGGIO: StatoRegistro = { stato: 'fermo', errore: null };
const senzaIscrizione = () => () => undefined;

/**
 * Pulsanti Accetta e Scarta (con motivo dalla lista chiusa e nota): aspettano la coda dei salvataggi dell'editor,
 * mandano il contenuto mostrato a schermo e il tempo finale, e non accettano dopo un salvataggio fallito.
 */
export function EsitoRevisione({ candidato, servizi, cronometro, onConcluso }: EsitoRevisioneProps) {
  const idMotivo = useId();
  const idNota = useId();
  const [motivo, setMotivo] = useState('');
  const [nota, setNota] = useState('');
  const [errore, setErrore] = useState<string | null>(null);
  const [inCorso, setInCorso] = useState(false);
  const registro = useContext(ContestoRegistro);
  const salvataggio = useSyncExternalStore(
    registro ? registro.iscriviti : senzaIscrizione,
    registro ? registro.istantanea : () => NESSUN_SALVATAGGIO,
  );
  const bloccato = salvataggio.stato !== 'fermo';

  async function concludi(esito: Esito) {
    setInCorso(true);
    setErrore(null);
    try {
      const maniglia = registro?.maniglia() ?? null;
      await maniglia?.attendiCoda();
      const erroreSalvataggio = maniglia?.errore() ?? null;
      if (esito === 'accettato' && erroreSalvataggio !== null) {
        setErrore(`Salvataggio non riuscito: ${erroreSalvataggio}`);
        return;
      }

      // Uno scarto vale anche con modifiche non valide: in quel caso non si manda contenuto e sul server resta l'ultima versione valida salvata.
      const misura = cronometro.concludi();
      const contenutoFinale = maniglia && erroreSalvataggio === null ? { contenutoFinale: maniglia.contenuto() } : {};
      if (esito === 'accettato') await servizi.accetta(candidato.id, { ...misura, ...contenutoFinale });
      else await servizi.scarta(candidato.id, { ...misura, motivo, nota, ...contenutoFinale });
      cronometro.ferma();
      onConcluso(esito);
    } catch (e) {
      setErrore(messaggioDiErrore(e));
    } finally {
      setInCorso(false);
    }
  }

  return (
    <section aria-labelledby={`${idMotivo}-titolo`} className="esito">
      <h3 id={`${idMotivo}-titolo`}>Esito della revisione</h3>
      <div className="esito-accetta">
        <button type="button" className="primario" disabled={inCorso || bloccato} onClick={() => void concludi('accettato')}>
          Accetta
        </button>
      </div>
      {salvataggio.stato === 'errore' ? (
        <p className="errore">Accetta è bloccato: l&apos;ultimo salvataggio non è riuscito. {salvataggio.errore}</p>
      ) : null}
      <div className="esito-scarta">
        <div className="campo">
          <label htmlFor={idMotivo}>Motivo dello scarto</label>
          <select id={idMotivo} value={motivo} onChange={(e) => setMotivo(e.target.value)}>
            <option value="">Scegli un motivo</option>
            {MOTIVI_SCARTO.map((m) => (
              <option key={m.codice} value={m.codice}>
                {m.etichetta}
              </option>
            ))}
          </select>
        </div>
        <div className="campo">
          <label htmlFor={idNota}>Nota (facoltativa)</label>
          <textarea id={idNota} rows={2} maxLength={MASSIMO_NOTA} value={nota} onChange={(e) => setNota(e.target.value)} />
        </div>
        <button type="button" className="secondario" disabled={inCorso || salvataggio.stato === 'in-corso' || motivo === ''} onClick={() => void concludi('scartato')}>
          Scarta
        </button>
      </div>
      {errore ? (
        <p role="alert" className="errore">
          {errore}
        </p>
      ) : null}
    </section>
  );
}
