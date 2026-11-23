import { useRef } from 'react';
import type { KeyboardEvent, ReactNode } from 'react';
import { AvvisoSalvataggio } from '../../comune/AvvisoSalvataggio';
import { CampoModificabile } from '../../comune/CampoModificabile';
import { ElencoControlli } from '../../comune/ElencoControlli';
import type { Api } from '../../comune/api';
import type { Candidato } from '../../comune/tipi';
import { conDati, useSalvataggio } from '../../comune/useSalvataggio';
import {
  CASELLA_NERA,
  CASELLA_VUOTA,
  conDefinizione,
  impostaLettera,
  leggiDefinizioni,
  leggiGriglia,
  numeroDellaCasella,
  soluzioniDallaGriglia,
} from './cruciverba';
import { AvvisiDellAutore } from './EditoreCampi';

interface EditoreCruciverbaProps {
  candidato: Candidato;
  servizi: Pick<Api, 'salvaFinale'>;
  children?: ReactNode;
}

const SPOSTAMENTI: Readonly<Record<string, [number, number]>> = {
  ArrowUp: [-1, 0],
  ArrowDown: [1, 0],
  ArrowLeft: [0, -1],
  ArrowRight: [0, 1],
};

/** Griglia con lo schema delle caselle fisso (si cambiano solo le lettere), definizioni e soluzioni. */
export function EditoreCruciverba({ candidato, servizi, children }: EditoreCruciverbaProps) {
  const { contenuto, controlli, stato, errore, modifica } = useSalvataggio(candidato, servizi);
  const griglia = leggiGriglia(contenuto.dati);
  const definizioni = leggiDefinizioni(contenuto.dati);
  const celle = useRef(new Map<string, HTMLInputElement>());
  const chiave = (r: number, c: number) => `${r}:${c}`;

  const sposta = (riga: number, colonna: number, dr: number, dc: number) => {
    let r = riga + dr;
    let c = colonna + dc;
    while (r >= 0 && r < griglia.righe && c >= 0 && c < griglia.colonne) {
      const campo = celle.current.get(chiave(r, c));
      if (campo) {
        campo.focus();
        return;
      }
      r += dr;
      c += dc;
    }
  };

  const allaPressione = (riga: number, colonna: number) => (e: KeyboardEvent<HTMLInputElement>) => {
    const spostamento = SPOSTAMENTI[e.key];
    if (spostamento) {
      e.preventDefault();
      sposta(riga, colonna, spostamento[0], spostamento[1]);
    }
  };

  const cambiaLettera = (riga: number, colonna: number, valore: string) => {
    modifica((c) => conDati(c, { griglia: impostaLettera(leggiGriglia(c.dati), riga, colonna, valore) }));
    if (valore !== '') sposta(riga, colonna, 0, 1);
  };

  const copiaSoluzioni = () =>
    modifica((c) => {
      const g = leggiGriglia(c.dati);
      return conDati(c, { definizioni: soluzioniDallaGriglia(g, leggiDefinizioni(c.dati), candidato.schemaCaselle?.parole ?? []) });
    });

  return (
    <div className="editore">
      <div className="cruciverba" role="group" aria-label="Griglia del cruciverba">
        {griglia.celle.map((riga, r) => (
          <div className="cruciverba-riga" key={r}>
            {[...riga].map((carattere, c) =>
              carattere === CASELLA_NERA ? (
                <div className="casella nera" key={c} aria-hidden="true" />
              ) : (
                <div className="casella" key={c}>
                  {numeroDellaCasella(griglia, r, c) !== null ? <span className="numero">{numeroDellaCasella(griglia, r, c)}</span> : null}
                  <input
                    ref={(el) => {
                      if (el) celle.current.set(chiave(r, c), el);
                      else celle.current.delete(chiave(r, c));
                    }}
                    type="text"
                    maxLength={2}
                    autoComplete="off"
                    aria-label={`Riga ${r + 1}, colonna ${c + 1}`}
                    value={carattere === CASELLA_VUOTA ? '' : carattere}
                    onChange={(e) => cambiaLettera(r, c, e.target.value)}
                    onKeyDown={allaPressione(r, c)}
                  />
                </div>
              ),
            )}
          </div>
        ))}
      </div>

      <section aria-labelledby="titolo-definizioni">
        <h3 id="titolo-definizioni">Definizioni</h3>
        <button type="button" className="secondario" onClick={copiaSoluzioni}>
          Copia le lettere della griglia nelle soluzioni
        </button>
        <ul className="definizioni">
          {definizioni.map((d, i) => (
            <li key={`${d.numero}-${d.direzione}`}>
              <CampoModificabile
                etichetta={`${d.numero} ${d.direzione}: definizione`}
                valore={d.testo}
                righe={2}
                onCambia={(v) => modifica((c) => conDati(c, { definizioni: conDefinizione(leggiDefinizioni(c.dati), i, { testo: v }) }))}
              />
              <CampoModificabile
                etichetta={`${d.numero} ${d.direzione}: soluzione`}
                valore={d.soluzione}
                onCambia={(v) =>
                  modifica((c) => conDati(c, { definizioni: conDefinizione(leggiDefinizioni(c.dati), i, { soluzione: v.toUpperCase() }) }))
                }
              />
            </li>
          ))}
        </ul>
      </section>

      <AvvisoSalvataggio stato={stato} errore={errore} />
      <AvvisiDellAutore avvisi={contenuto.avvisi} />
      <ElencoControlli controlli={controlli} />
      {children}
    </div>
  );
}
