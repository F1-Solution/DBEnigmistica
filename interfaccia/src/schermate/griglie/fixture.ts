import { ErroreApi } from '../../comune/api';
import type { Api } from '../../comune/api';
import type { Candidato, Dati, Involucro } from '../../comune/tipi';
import { candidatoProva } from '../../prova/apiFinta';

// Dati di prova inventati, solo per i test delle schermate.

export function involucro(tipologia: string, dati: Dati): Involucro {
  return { versione_formato: 1, tipologia, versione_tipologia: 0, titolo: null, dati, avvisi: [] };
}

export function candidato(contenuto: Involucro, controlli: string[] = []): Candidato {
  return candidatoProva({ tipologia: contenuto.tipologia, modo: 'algoritmo', contenutoGenerato: contenuto, contenutoFinale: null, controlli });
}

type ServizioFinto = Pick<Api, 'salvaFinale'> & { chiamate: Involucro[] };

/** Finto di `PUT .../finale`: registra i contenuti ricevuti e risponde con i controlli indicati. */
export function servizioFinto(controlli: string[] = []): ServizioFinto {
  const chiamate: Involucro[] = [];
  return {
    chiamate,
    salvaFinale: (_id, contenuto) => {
      chiamate.push(contenuto);
      return Promise.resolve(candidatoProva({ tipologia: contenuto.tipologia, contenutoFinale: contenuto, controlli }));
    },
  };
}

/** Come l'host: i problemi di coerenza e unicità dei giochi logici tornano come errore 422 con il messaggio. */
export function servizioCheRifiuta(messaggio: string): ServizioFinto {
  const chiamate: Involucro[] = [];
  return {
    chiamate,
    salvaFinale: (_id, contenuto) => {
      chiamate.push(contenuto);
      return Promise.reject(new ErroreApi('Richiesta non riuscita (codice 422).', 422, [messaggio]));
    },
  };
}

const RIGHE_SUDOKU = [
  '534678912', '672195348', '198342567', '859761423', '426853791', '713924856', '961537284', '287419635', '345286179',
];

export function sudoku(): Involucro {
  const schema = RIGHE_SUDOKU.map((_, i) => (i === 0 ? '53..7....' : '.'.repeat(9)));
  return involucro('sudoku', {
    righe: 9,
    colonne: 9,
    griglia: { righe: 9, colonne: 9, celle: schema, numeri: [] },
    soluzione_griglia: { righe: 9, colonne: 9, celle: RIGHE_SUDOKU, numeri: [] },
    difficolta: 2,
  });
}

export function futoshiki(): Involucro {
  const soluzione = ['12345', '23451', '34512', '45123', '51234'];
  return involucro('futoshiki', {
    righe: 5,
    colonne: 5,
    griglia: { righe: 5, colonne: 5, celle: ['.....', '.....', '.....', '.....', '.....'], numeri: [] },
    soluzione_griglia: { righe: 5, colonne: 5, celle: soluzione, numeri: [] },
    difficolta: 3,
    segni: [{ riga: 1, colonna: 1, verso: 'destra', segno: 'minore' }],
  });
}

export function nonogramma(): Involucro {
  const disegno = ['.###.', '#...#', '#####', '#...#', '.....'];
  return involucro('nonogramma', {
    righe: 5,
    colonne: 5,
    griglia: { righe: 5, colonne: 5, celle: Array(5).fill('.....'), numeri: [] },
    soluzione_griglia: { righe: 5, colonne: 5, celle: disegno, numeri: [] },
    difficolta: 1,
    indizi_righe: [{ indice: 1, blocchi: '3' }],
    indizi_colonne: [{ indice: 1, blocchi: '3' }],
  });
}

export function rebus(): Involucro {
  return involucro('rebus', {
    esposto: 'Una scena inventata con due figure.',
    diagramma: '2 4',
    prima_lettura: 'LA luna',
    soluzione: 'La luna',
    spiegazione: null,
    vignette: [
      { vignetta: 1, descrizione: 'Un disco chiaro nel cielo notturno.', lettere: 'LA', parola: 'luna' },
    ],
  });
}

/** Contenuto inviato al server alla chiamata i-esima; fallisce se non c'è stata. */
export function chiamata(servizi: { chiamate: Involucro[] }, i: number): Involucro {
  const c = servizi.chiamate[i];
  if (!c) throw new Error(`Nessun salvataggio numero ${i + 1}.`);
  return c;
}
