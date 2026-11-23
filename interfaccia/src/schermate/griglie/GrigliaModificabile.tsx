import { useRef } from 'react';
import type { KeyboardEvent, ReactNode } from 'react';
import type { Griglia, Segno, TipoSegno, Verso } from './griglia';
import { descriviSegno, simboloSegno } from './griglia';
import './griglie.css';

interface Props {
  etichetta: string;
  griglia: Griglia;
  /** Modo "caratteri": si scrive un carattere ammesso. Modo "alterna": clic o spazio alternano pieno e vuoto. */
  modo: 'caratteri' | 'alterna';
  ammessi?: string;
  vuoto?: string;
  riquadro?: number;
  indiziRighe?: string[];
  indiziColonne?: string[];
  segni?: readonly Segno[];
  onCambia: (riga: number, colonna: number, carattere: string) => void;
  onCambiaSegno?: (riga: number, colonna: number, verso: Verso) => void;
}

function segnoDi(segni: readonly Segno[] | undefined, riga: number, colonna: number, verso: Verso): TipoSegno | null {
  return segni?.find((s) => s.riga === riga + 1 && s.colonna === colonna + 1 && s.verso === verso)?.segno ?? null;
}

const FRECCE: Record<string, [number, number]> = {
  ArrowUp: [-1, 0], ArrowDown: [1, 0], ArrowLeft: [0, -1], ArrowRight: [0, 1],
};

export function GrigliaModificabile({
  etichetta, griglia, modo, ammessi = '', vuoto = '.', riquadro, indiziRighe, indiziColonne, segni, onCambia, onCambiaSegno,
}: Props) {
  const radice = useRef<HTMLDivElement>(null);
  const { righe, colonne, celle } = griglia;
  const conSegni = segni !== undefined && onCambiaSegno !== undefined;

  function fuoco(r: number, c: number) {
    const rr = Math.max(0, Math.min(righe - 1, r));
    const cc = Math.max(0, Math.min(colonne - 1, c));
    radice.current?.querySelectorAll<HTMLElement>('[data-cella]')[rr * colonne + cc]?.focus();
  }

  function tasto(e: KeyboardEvent<HTMLElement>, r: number, c: number) {
    const freccia = FRECCE[e.key];
    if (freccia) {
      e.preventDefault();
      fuoco(r + freccia[0], c + freccia[1]);
      return;
    }
    if (e.ctrlKey || e.metaKey || e.altKey) return;
    if (modo === 'alterna') {
      // Spazio e Invio attivano già il clic del pulsante.
      if (e.key === '#' || e.key === '.') {
        e.preventDefault();
        onCambia(r, c, e.key);
      }
      return;
    }
    if (e.key === 'Backspace' || e.key === 'Delete' || e.key === vuoto) {
      e.preventDefault();
      onCambia(r, c, vuoto);
    } else if (e.key.length === 1 && ammessi.includes(e.key)) {
      e.preventDefault();
      onCambia(r, c, e.key);
    }
  }

  function cella(r: number, c: number): ReactNode {
    const valore = celle[r]?.[c] ?? vuoto;
    const destro = riquadro && (c + 1) % riquadro === 0 && c < colonne - 1 ? ' gr-bordo-destro' : '';
    const sotto = riquadro && (r + 1) % riquadro === 0 && r < righe - 1 ? ' gr-bordo-sotto' : '';
    const descrizione = valore === vuoto || (modo === 'alterna' && valore !== '#') ? 'vuota' : valore === '#' ? 'piena' : valore;
    return (
      <button
        key={`c${r}-${c}`}
        type="button"
        data-cella=""
        className={`gr-cella${valore === '#' ? ' gr-piena' : ''}${destro}${sotto}`}
        aria-label={`${etichetta}, riga ${r + 1}, colonna ${c + 1}: ${descrizione}`}
        onKeyDown={(e) => tasto(e, r, c)}
        onClick={modo === 'alterna' ? () => onCambia(r, c, valore === '#' ? '.' : '#') : undefined}
      >
        {modo === 'caratteri' && valore !== vuoto ? valore : ''}
      </button>
    );
  }

  function segno(r: number, c: number, verso: Verso): ReactNode {
    const s = segnoDi(segni, r, c, verso);
    return (
      <button
        key={`s${r}-${c}-${verso}`}
        type="button"
        className="gr-segno"
        aria-label={descriviSegno(r + 1, c + 1, verso, s)}
        onClick={() => onCambiaSegno?.(r + 1, c + 1, verso)}
      >
        {simboloSegno(verso, s)}
      </button>
    );
  }

  const figli: ReactNode[] = [];
  for (let r = 0; r < righe; r++) {
    for (let c = 0; c < colonne; c++) {
      figli.push(cella(r, c));
      if (conSegni && c < colonne - 1) figli.push(segno(r, c, 'destra'));
    }
    if (conSegni && r < righe - 1) {
      for (let c = 0; c < colonne; c++) {
        figli.push(segno(r, c, 'sotto'));
        if (c < colonne - 1) figli.push(<span key={`v${r}-${c}`} />);
      }
    }
  }

  const colonneCss = conSegni ? colonne * 2 - 1 : colonne;
  const modello = Array.from({ length: colonneCss }, (_, i) =>
    conSegni && i % 2 === 1 ? 'var(--gr-segno, 28px)' : 'var(--gr-cella, 36px)').join(' ');

  return (
    <div className="gr-blocco" ref={radice}>
      {indiziColonne && (
        <div className="gr-indizi gr-indizi-colonne" style={{ gridTemplateColumns: `repeat(${colonne}, var(--gr-cella, 36px))` }}>
          {indiziColonne.map((t, i) => (
            <span key={i} aria-label={`Indizio colonna ${i + 1}: ${t}`}>
              {t.split(' ').map((n, j) => <span key={j}>{n}</span>)}
            </span>
          ))}
        </div>
      )}
      <div className="gr-riga-indizi">
        {indiziRighe && (
          <div className="gr-indizi gr-indizi-righe">
            {indiziRighe.map((t, i) => (
              <span key={i} aria-label={`Indizio riga ${i + 1}: ${t}`}>{t}</span>
            ))}
          </div>
        )}
        <div role="group" aria-label={etichetta} className="gr-griglia" style={{ gridTemplateColumns: modello }}>
          {figli}
        </div>
      </div>
    </div>
  );
}
