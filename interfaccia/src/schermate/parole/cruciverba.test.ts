import { describe, expect, it } from 'vitest';
import type { ParolaSchema } from '../../comune/tipi';
import type { Definizione, GrigliaCruciverba } from './cruciverba';
import { impostaLettera, lettereDellaParola, normalizzaLettera, numeroDellaCasella, soluzioniDallaGriglia } from './cruciverba';

const griglia: GrigliaCruciverba = {
  righe: 3,
  colonne: 3,
  celle: ['ABC', 'D#E', 'FGH'],
  numeri: [
    { riga: 1, colonna: 1, numero: 1 },
    { riga: 1, colonna: 3, numero: 2 },
  ],
};

describe('cruciverba', () => {
  it('normalizza a una lettera maiuscola', () => {
    expect(normalizzaLettera('a')).toBe('A');
    expect(normalizzaLettera('xy')).toBe('Y');
    expect(normalizzaLettera('1')).toBe('.');
    expect(normalizzaLettera('')).toBe('.');
  });

  it('imposta una lettera senza cambiare la griglia di partenza', () => {
    const nuova = impostaLettera(griglia, 0, 1, 'z');
    expect(nuova.celle).toEqual(['AZC', 'D#E', 'FGH']);
    expect(griglia.celle[0]).toBe('ABC');
  });

  it('non tocca le caselle nere né quelle fuori griglia', () => {
    expect(impostaLettera(griglia, 1, 1, 'x')).toBe(griglia);
    expect(impostaLettera(griglia, 9, 9, 'x')).toBe(griglia);
  });

  it('trova il numero di una casella (coordinate da 0)', () => {
    expect(numeroDellaCasella(griglia, 0, 2)).toBe(2);
    expect(numeroDellaCasella(griglia, 2, 2)).toBeNull();
  });

  it('legge le lettere di una parola orizzontale e verticale', () => {
    const o: ParolaSchema = { numero: 1, direzione: 'orizzontale', riga: 1, colonna: 1, lunghezza: 3 };
    const v: ParolaSchema = { numero: 1, direzione: 'verticale', riga: 1, colonna: 1, lunghezza: 3 };
    expect(lettereDellaParola(griglia, o)).toBe('ABC');
    expect(lettereDellaParola(griglia, v)).toBe('ADF');
  });

  it('copia le lettere della griglia nelle soluzioni', () => {
    const definizioni: Definizione[] = [
      { numero: 1, direzione: 'orizzontale', testo: 'x', soluzione: 'ZZZ' },
      { numero: 1, direzione: 'verticale', testo: 'y', soluzione: 'ZZZ' },
    ];
    const parole: ParolaSchema[] = [
      { numero: 1, direzione: 'orizzontale', riga: 1, colonna: 1, lunghezza: 3 },
      { numero: 1, direzione: 'verticale', riga: 1, colonna: 1, lunghezza: 3 },
    ];
    expect(soluzioniDallaGriglia(griglia, definizioni, parole).map((d) => d.soluzione)).toEqual(['ABC', 'ADF']);
  });
});
