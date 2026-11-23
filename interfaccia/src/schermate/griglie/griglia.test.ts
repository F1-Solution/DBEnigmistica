import { describe, expect, it } from 'vitest';
import { calcolaIndizi, ciclaSegno, impostaCella, type Griglia, type Segno } from './griglia';

describe('impostaCella', () => {
  it('restituisce una nuova griglia senza toccare quella di partenza', () => {
    const g: Griglia = { righe: 2, colonne: 2, celle: ['..', '..'], numeri: [] };
    const nuova = impostaCella(g, 1, 0, '7');
    expect(nuova.celle).toEqual(['..', '7.']);
    expect(g.celle).toEqual(['..', '..']);
  });
});

describe('calcolaIndizi', () => {
  it('calcola i blocchi di righe e colonne, con 0 per le linee vuote', () => {
    const indizi = calcolaIndizi(['.###.', '#...#', '#####', '#...#', '.....']);
    expect(indizi.righe).toEqual(['3', '1 1', '5', '1 1', '0']);
    expect(indizi.colonne).toEqual(['3', '1 1', '1 1', '1 1', '3']);
  });
});

describe('ciclaSegno', () => {
  it('passa da nessun segno a maggiore, minore e di nuovo nessuno', () => {
    const a = ciclaSegno([], 1, 1, 'destra');
    expect(a).toEqual([{ riga: 1, colonna: 1, verso: 'destra', segno: 'maggiore' }]);
    const b = ciclaSegno(a, 1, 1, 'destra');
    expect(b[0]?.segno).toBe('minore');
    expect(ciclaSegno(b, 1, 1, 'destra')).toEqual([]);
  });

  it('mantiene l\'ordine del formato: riga, colonna, destra prima di sotto', () => {
    let segni: Segno[] = [];
    segni = ciclaSegno(segni, 2, 1, 'sotto');
    segni = ciclaSegno(segni, 1, 2, 'destra');
    segni = ciclaSegno(segni, 1, 1, 'sotto');
    segni = ciclaSegno(segni, 1, 1, 'destra');
    expect(segni.map((s) => `${s.riga}${s.colonna}${s.verso}`)).toEqual(['11destra', '11sotto', '12destra', '21sotto']);
  });
});
