import { describe, expect, it } from 'vitest';
import { costruisciBrief, limitiDimensioni } from './brief';
import type { CampoBrief, Tipologia } from './tipi';

const campo = (nome: CampoBrief['nome'], extra: Partial<CampoBrief> = {}): CampoBrief => ({
  nome,
  etichetta: nome,
  tipo: 'testo',
  obbligatorio: false,
  valori: [],
  minimo: null,
  massimo: null,
  ...extra,
});

const tipologia = (id: string, famiglia: string, campiBrief: CampoBrief[]): Tipologia => ({
  id,
  versione: 0,
  famiglia,
  nome: id,
  modo: 'ai',
  statoRegole: 'bozza',
  regoleApprovate: false,
  generazioneConsentita: false,
  motivoGenerazioneNonConsentita: null,
  campiBrief,
});

const valori = { tema: '  ', sottotipo: '', vincoli: '', difficolta: 3, righe: 9, colonne: 9 };

describe('costruisciBrief', () => {
  it('crittografia: sottotipo, tema libero null e vincoli una per riga', () => {
    const t = tipologia('crittografia-breve', 'crittografia', [campo('tema'), campo('sottotipo'), campo('vincoli'), campo('difficolta')]);

    expect(costruisciBrief(t, { ...valori, sottotipo: 'sinonimica', vincoli: ' nessun nome proprio \n\n solo parole comuni ' })).toEqual({
      versione_formato: 1,
      tipologia: 'crittografia-breve',
      versione_tipologia: 0,
      famiglia: 'crittografia',
      tema: null,
      vincoli: ['nessun nome proprio', 'solo parole comuni'],
      difficolta: 3,
      dimensioni: null,
      sottotipo: 'sinonimica',
    });
  });

  it('giochi logici: niente tema né vincoli, dimensioni presenti', () => {
    const t = tipologia('nonogramma', 'logico', [campo('difficolta'), campo('dimensioni', { tipo: 'dimensioni', minimo: 10, massimo: 15 })]);

    const brief = costruisciBrief(t, { ...valori, tema: 'inutile', vincoli: 'inutile', righe: 10, colonne: 15 });

    expect(brief).toMatchObject({ tema: null, vincoli: [], dimensioni: { righe: 10, colonne: 15 }, sottotipo: null });
  });
});

describe('limitiDimensioni', () => {
  it('quadrate: i lati ammessi', () => {
    const t = tipologia('futoshiki', 'logico', [campo('dimensioni', { tipo: 'dimensioni-quadrate', valori: ['5', '6', '7'] })]);
    expect(limitiDimensioni(t)).toEqual({ lati: [5, 6, 7], minimo: 5, massimo: 7 });
  });

  it('senza campo dimensioni non ci sono limiti', () => {
    expect(limitiDimensioni(tipologia('anagramma', 'gioco-breve', []))).toBeNull();
  });
});
