import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import type { Candidato } from '../../comune/tipi';
import { apiFinta, candidatoProva } from '../../prova/apiFinta';
import { EditoreCruciverba } from './EditoreCruciverba';

function cruciverba(): Candidato {
  return candidatoProva({
    tipologia: 'cruciverba-classico',
    schemaCaselle: {
      righe: 2,
      colonne: 2,
      celle: ['..', '..'],
      parole: [
        { numero: 1, direzione: 'orizzontale', riga: 1, colonna: 1, lunghezza: 2 },
        { numero: 1, direzione: 'verticale', riga: 1, colonna: 1, lunghezza: 2 },
      ],
    },
    contenutoGenerato: {
      versione_formato: 1,
      tipologia: 'cruciverba-classico',
      versione_tipologia: 1,
      titolo: null,
      avvisi: [],
      dati: {
        righe: 2,
        colonne: 2,
        griglia: { righe: 2, colonne: 2, celle: ['AB', 'C#'], numeri: [{ riga: 1, colonna: 1, numero: 1 }] },
        autore: null,
        definizioni: [{ numero: 1, direzione: 'orizzontale', testo: 'Prime lettere', soluzione: 'AB' }],
      },
    },
  });
}

describe('EditoreCruciverba', () => {
  it('mostra lo schema fisso: le caselle nere non si modificano e i numeri si vedono', () => {
    render(<EditoreCruciverba candidato={cruciverba()} servizi={apiFinta()} />);

    expect(screen.getAllByRole('textbox', { name: /Riga \d, colonna \d/ })).toHaveLength(3);
    expect(screen.queryByLabelText('Riga 2, colonna 2')).not.toBeInTheDocument();
    expect(screen.getByText('1', { selector: '.numero' })).toBeInTheDocument();
  });

  it('una lettera da tastiera si salva nella forma del descrittore', async () => {
    const api = apiFinta();
    render(<EditoreCruciverba candidato={cruciverba()} servizi={api} />);

    const casella = screen.getByLabelText('Riga 1, colonna 2');
    await userEvent.type(casella, 'z');

    await waitFor(() => expect(api.salvaFinale).toHaveBeenCalled());
    const [, contenuto] = api.salvaFinale.mock.calls.at(-1) as [string, { dati: { griglia: { celle: string[] } } }];
    expect(contenuto.dati.griglia.celle).toEqual(['AZ', 'C#']);
  });

  it('le frecce spostano il fuoco', async () => {
    render(<EditoreCruciverba candidato={cruciverba()} servizi={apiFinta()} />);
    screen.getByLabelText('Riga 1, colonna 1').focus();

    await userEvent.keyboard('{ArrowRight}');
    expect(screen.getByLabelText('Riga 1, colonna 2')).toHaveFocus();
    await userEvent.keyboard('{ArrowDown}');
    expect(screen.queryByLabelText('Riga 2, colonna 2')).not.toBeInTheDocument();
  });

  it('copia le lettere della griglia nelle soluzioni', async () => {
    const api = apiFinta();
    const candidato = cruciverba();
    render(<EditoreCruciverba candidato={candidato} servizi={api} />);
    await userEvent.type(screen.getByLabelText('Riga 1, colonna 2'), 'q');
    await userEvent.click(screen.getByRole('button', { name: /Copia le lettere/ }));

    await waitFor(() => expect(api.salvaFinale.mock.calls.length).toBeGreaterThanOrEqual(2));
    const [, contenuto] = api.salvaFinale.mock.calls.at(-1) as [string, { dati: { definizioni: { soluzione: string }[] } }];
    expect(contenuto.dati.definizioni[0]?.soluzione).toBe('AQ');
  });
});
