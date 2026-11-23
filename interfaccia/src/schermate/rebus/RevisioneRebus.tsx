import type { ReactNode } from 'react';
import type { Api } from '../../comune/api';
import { AvvisoSalvataggio } from '../../comune/AvvisoSalvataggio';
import { ElencoControlli } from '../../comune/ElencoControlli';
import type { Candidato, Dati } from '../../comune/tipi';
import { conDati, useSalvataggio } from '../../comune/useSalvataggio';
import '../griglie/griglie.css';
import { aggiungiVignetta, aggiornaVignetta, leggiVignette, rimuoviVignetta } from './rebus';

interface Props {
  candidato: Candidato;
  servizi: Pick<Api, 'salvaFinale'>;
  /** Spazio per cronometro, Accetta e Scarta dei componenti comuni. */
  children?: ReactNode;
}

const CAMPI: { nome: string; etichetta: string; lungo?: boolean; facoltativo?: boolean }[] = [
  { nome: 'esposto', etichetta: 'Scena', lungo: true },
  { nome: 'diagramma', etichetta: 'Diagramma' },
  { nome: 'prima_lettura', etichetta: 'Prima lettura' },
  { nome: 'soluzione', etichetta: 'Soluzione' },
  { nome: 'spiegazione', etichetta: 'Spiegazione', lungo: true, facoltativo: true },
];

function testo(dati: Dati, nome: string): string {
  const v = dati[nome];
  return typeof v === 'string' ? v : '';
}

interface CampoProps {
  etichetta: string;
  valore: string;
  lungo?: boolean;
  onCambia: (valore: string) => void;
}

// Ogni modifica va in coda di salvataggio: Accetta resta disabilitato finché non è tutto salvato.
function CampoTesto({ etichetta, valore, lungo, onCambia }: CampoProps) {
  const comune = { value: valore, onChange: (e: { target: { value: string } }) => onCambia(e.target.value) };
  return (
    <label className="gr-campo">
      {etichetta}
      {lungo ? <textarea rows={3} {...comune} /> : <input type="text" {...comune} />}
    </label>
  );
}

/** Rebus descritto a parole: nessuna immagine e nessun campo file (criterio 8). */
export function RevisioneRebus({ candidato, servizi, children }: Props) {
  const salvataggio = useSalvataggio(candidato, servizi);
  const { contenuto, modifica } = salvataggio;
  const dati = contenuto.dati;
  const vignette = leggiVignette(dati);

  const salvaCampo = (nome: string, facoltativo?: boolean) => (valore: string) =>
    modifica((c) => conDati(c, { [nome]: facoltativo && valore.trim() === '' ? null : valore }));

  const salvaVignette = (cambia: (v: ReturnType<typeof leggiVignette>) => ReturnType<typeof leggiVignette>) =>
    modifica((c) => conDati(c, { vignette: cambia(leggiVignette(c.dati)) }));

  return (
    <div>
      {CAMPI.map((c) => (
        <CampoTesto key={c.nome} etichetta={c.etichetta} valore={testo(dati, c.nome)} lungo={c.lungo} onCambia={salvaCampo(c.nome, c.facoltativo)} />
      ))}
      <h3>Figure delle vignette</h3>
      {vignette.map((v, i) => (
        <fieldset key={i} className="gr-voce">
          <legend>Figura {i + 1}</legend>
          <label className="gr-campo">
            Vignetta
            <select value={v.vignetta} onChange={(e) => salvaVignette((l) => aggiornaVignetta(l, i, { vignetta: Number(e.target.value) }))}>
              {[1, 2, 3].map((n) => <option key={n} value={n}>{n}</option>)}
            </select>
          </label>
          <CampoTesto etichetta="Descrizione" valore={v.descrizione} lungo onCambia={(t) => salvaVignette((l) => aggiornaVignetta(l, i, { descrizione: t }))} />
          <CampoTesto etichetta="Lettere" valore={v.lettere ?? ''} onCambia={(t) => salvaVignette((l) => aggiornaVignetta(l, i, { lettere: t.trim() === '' ? null : t }))} />
          <CampoTesto etichetta="Parola letta" valore={v.parola} onCambia={(t) => salvaVignette((l) => aggiornaVignetta(l, i, { parola: t }))} />
          <button type="button" onClick={() => salvaVignette((l) => rimuoviVignetta(l, i))}>Rimuovi figura {i + 1}</button>
        </fieldset>
      ))}
      <button type="button" onClick={() => salvaVignette(aggiungiVignetta)}>Aggiungi figura</button>
      <AvvisoSalvataggio stato={salvataggio.stato} errore={salvataggio.errore} />
      <ElencoControlli controlli={salvataggio.controlli} />
      {children}
    </div>
  );
}
