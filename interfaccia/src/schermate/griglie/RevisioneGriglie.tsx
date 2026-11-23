import type { ReactNode } from 'react';
import type { Api } from '../../comune/api';
import { AvvisoSalvataggio } from '../../comune/AvvisoSalvataggio';
import { ElencoControlli } from '../../comune/ElencoControlli';
import type { Candidato, Dati } from '../../comune/tipi';
import { conDati, useSalvataggio } from '../../comune/useSalvataggio';
import { GrigliaModificabile } from './GrigliaModificabile';
import type { Griglia, Segno } from './griglia';
import { calcolaIndizi, ciclaSegno, impostaCella, indiziComeElenco, leggiGriglia } from './griglia';

export const TIPOLOGIE_GRIGLIE = ['sudoku', 'futoshiki', 'nonogramma'] as const;

interface Props {
  candidato: Candidato;
  servizi: Pick<Api, 'salvaFinale'>;
  /** Spazio per cronometro, Accetta e Scarta dei componenti comuni. */
  children?: ReactNode;
}

function cifre(n: number): string {
  return Array.from({ length: n }, (_, i) => String(i + 1)).join('');
}

function numero(dati: Dati, nome: string): number {
  const v = dati[nome];
  if (typeof v !== 'number') throw new Error(`Il campo «${nome}» non è un numero.`);
  return v;
}

export function RevisioneGriglie({ candidato, servizi, children }: Props) {
  const salvataggio = useSalvataggio(candidato, servizi);
  const { contenuto, modifica } = salvataggio;
  const tipologia = contenuto.tipologia;
  const dati = contenuto.dati;

  const cambiaGriglia = (nome: string) => (riga: number, colonna: number, carattere: string) =>
    modifica((c) => conDati(c, { [nome]: impostaCella(leggiGriglia(c.dati, nome), riga, colonna, carattere) }));

  const cambiaDisegno = (riga: number, colonna: number, carattere: string) =>
    modifica((c) => aggiornaDisegno(c, impostaCella(leggiGriglia(c.dati, 'soluzione_griglia'), riga, colonna, carattere)));

  function aggiornaDisegno(c: typeof contenuto, disegno: Griglia) {
    const indizi = calcolaIndizi(disegno.celle);
    return conDati(c, {
      soluzione_griglia: disegno,
      indizi_righe: indiziComeElenco(indizi.righe),
      indizi_colonne: indiziComeElenco(indizi.colonne),
    });
  }

  const cambiaSegno = (riga: number, colonna: number, verso: 'destra' | 'sotto') =>
    modifica((c) => conDati(c, { segni: ciclaSegno((c.dati.segni as Segno[]) ?? [], riga, colonna, verso) }));

  return (
    <div>
      <p>Difficoltà stimata: {String(dati.difficolta ?? '–')}</p>
      {tipologia === 'sudoku' && (
        <>
          <h3>Schema da giocare</h3>
          <GrigliaModificabile etichetta="Schema" griglia={leggiGriglia(dati, 'griglia')} modo="caratteri" ammessi={cifre(9)} riquadro={3}
            onCambia={cambiaGriglia('griglia')} />
          <h3>Soluzione</h3>
          <GrigliaModificabile etichetta="Soluzione" griglia={leggiGriglia(dati, 'soluzione_griglia')} modo="caratteri" ammessi={cifre(9)} riquadro={3}
            onCambia={cambiaGriglia('soluzione_griglia')} />
        </>
      )}
      {tipologia === 'futoshiki' && (
        <>
          <h3>Schema da giocare, con i segni tra le caselle</h3>
          <GrigliaModificabile etichetta="Schema" griglia={leggiGriglia(dati, 'griglia')} modo="caratteri" ammessi={cifre(numero(dati, 'righe'))}
            segni={(dati.segni as Segno[]) ?? []} onCambia={cambiaGriglia('griglia')} onCambiaSegno={cambiaSegno} />
          <h3>Soluzione</h3>
          <GrigliaModificabile etichetta="Soluzione" griglia={leggiGriglia(dati, 'soluzione_griglia')} modo="caratteri" ammessi={cifre(numero(dati, 'righe'))}
            onCambia={cambiaGriglia('soluzione_griglia')} />
        </>
      )}
      {tipologia === 'nonogramma' && (
        <>
          <h3>Disegno e indizi ai margini</h3>
          <GrigliaModificabile etichetta="Disegno" griglia={leggiGriglia(dati, 'soluzione_griglia')} modo="alterna"
            indiziRighe={calcolaIndizi(leggiGriglia(dati, 'soluzione_griglia').celle).righe}
            indiziColonne={calcolaIndizi(leggiGriglia(dati, 'soluzione_griglia').celle).colonne}
            onCambia={cambiaDisegno} />
        </>
      )}
      {!TIPOLOGIE_GRIGLIE.includes(tipologia as (typeof TIPOLOGIE_GRIGLIE)[number]) && <p role="alert">Tipologia non gestita: {tipologia}.</p>}
      <AvvisoSalvataggio stato={salvataggio.stato} errore={salvataggio.errore} />
      <ElencoControlli controlli={salvataggio.controlli} />
      {children}
    </div>
  );
}
