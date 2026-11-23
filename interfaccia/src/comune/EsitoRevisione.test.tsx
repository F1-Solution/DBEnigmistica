import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { ErroreApi } from './api';
import { EsitoRevisione } from './EsitoRevisione';
import { ContestoRegistro, creaRegistro } from './registroSalvataggio';
import type { RegistroSalvataggio } from './registroSalvataggio';
import type { Involucro } from './tipi';
import { involucroCrittografia } from '../prova/apiFinta';

const misura = { tempoAttivoMs: 160_000, ultimaAttivitaIl: '2026-12-01T10:00:00.000Z' };

function prepara(opzioni: { accetta?: () => Promise<never>; registro?: RegistroSalvataggio } = {}) {
  const cronometro = { concludi: vi.fn(() => misura), ferma: vi.fn() };
  const servizi = {
    accetta: vi.fn(opzioni.accetta ?? (() => Promise.resolve({} as never))),
    scarta: vi.fn(() => Promise.resolve({} as never)),
  };
  const onConcluso = vi.fn();
  const esito = <EsitoRevisione candidato={{ id: 'c1' }} servizi={servizi} cronometro={cronometro} onConcluso={onConcluso} />;
  render(opzioni.registro ? <ContestoRegistro.Provider value={opzioni.registro}>{esito}</ContestoRegistro.Provider> : esito);
  return { cronometro, servizi, onConcluso };
}

/** Registro con una maniglia finta: coda controllabile, contenuto mostrato e ultimo errore. */
function registroCon(contenuto: Involucro, opzioni: { errore?: string | null; coda?: () => Promise<void> } = {}) {
  const registro = creaRegistro();
  registro.registra({
    attendiCoda: opzioni.coda ?? (() => Promise.resolve()),
    contenuto: () => contenuto,
    errore: () => opzioni.errore ?? null,
  });
  return registro;
}

describe('EsitoRevisione', () => {
  it('Accetta manda il tempo finale, ferma il cronometro e segnala la conclusione', async () => {
    const { cronometro, servizi, onConcluso } = prepara();

    await userEvent.click(screen.getByRole('button', { name: 'Accetta' }));

    expect(servizi.accetta).toHaveBeenCalledWith('c1', misura);
    expect(cronometro.ferma).toHaveBeenCalledTimes(1);
    expect(onConcluso).toHaveBeenCalledWith('accettato');
  });

  it('Scarta richiede un motivo e manda motivo, nota e tempo', async () => {
    const { servizi, onConcluso } = prepara();
    const scarta = screen.getByRole('button', { name: 'Scarta' });
    expect(scarta).toBeDisabled();

    await userEvent.selectOptions(screen.getByLabelText('Motivo dello scarto'), 'soluzione_errata');
    await userEvent.type(screen.getByLabelText('Nota (facoltativa)'), 'cena non è un pasto');
    await userEvent.click(scarta);

    expect(servizi.scarta).toHaveBeenCalledWith('c1', { ...misura, motivo: 'soluzione_errata', nota: 'cena non è un pasto' });
    expect(onConcluso).toHaveBeenCalledWith('scartato');
  });

  it("un errore dell'host si mostra e non ferma il cronometro", async () => {
    const { cronometro, onConcluso } = prepara({
      accetta: () => Promise.reject(new ErroreApi('no', 422, ['Il contenuto non è valido.'])),
    });

    await userEvent.click(screen.getByRole('button', { name: 'Accetta' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('Il contenuto non è valido.');
    expect(cronometro.ferma).not.toHaveBeenCalled();
    expect(onConcluso).not.toHaveBeenCalled();
  });

  it("modifica e Accetta subito: aspetta la coda e manda l'ultimo contenuto mostrato", async () => {
    let finita = false;
    const ultimo = involucroCrittografia('PRANZO DEL SOVRANO');
    const registro = registroCon(ultimo, {
      coda: () =>
        new Promise<void>((risolvi) =>
          setTimeout(() => {
            finita = true;
            risolvi();
          }, 20),
        ),
    });
    const { servizi } = prepara({ registro });

    await userEvent.click(screen.getByRole('button', { name: 'Accetta' }));

    await waitFor(() => expect(servizi.accetta).toHaveBeenCalledWith('c1', { ...misura, contenutoFinale: ultimo }));
    expect(finita).toBe(true);
  });

  it('Scarta aspetta la coda e manda il contenuto mostrato', async () => {
    const ultimo = involucroCrittografia('ALTRO');
    const { servizi } = prepara({ registro: registroCon(ultimo) });

    await userEvent.selectOptions(screen.getByLabelText('Motivo dello scarto'), 'altro');
    await userEvent.click(screen.getByRole('button', { name: 'Scarta' }));

    expect(servizi.scarta).toHaveBeenCalledWith('c1', { ...misura, motivo: 'altro', nota: '', contenutoFinale: ultimo });
  });

  it("dopo un salvataggio fallito (422) Accetta è disabilitato e non accetta: mostra il motivo dell'host", async () => {
    const registro = registroCon(involucroCrittografia(), { errore: 'La soluzione non ha le lettere della prima lettura.' });
    registro.imposta({ stato: 'errore', errore: 'La soluzione non ha le lettere della prima lettura.' });
    const { servizi, onConcluso } = prepara({ registro });

    const accetta = screen.getByRole('button', { name: 'Accetta' });
    expect(accetta).toBeDisabled();
    expect(screen.getByText(/La soluzione non ha le lettere della prima lettura\./)).toBeInTheDocument();
    await userEvent.click(accetta);

    expect(servizi.accetta).not.toHaveBeenCalled();
    expect(onConcluso).not.toHaveBeenCalled();
  });

  it("se l'ultimo salvataggio fallisce mentre si aspetta la coda, non accetta", async () => {
    const registro = registroCon(involucroCrittografia(), { errore: 'Non valido.' });
    const { servizi, onConcluso } = prepara({ registro });

    await userEvent.click(screen.getByRole('button', { name: 'Accetta' }));

    expect(servizi.accetta).not.toHaveBeenCalled();
    expect(onConcluso).not.toHaveBeenCalled();
    expect(await screen.findByRole('alert')).toHaveTextContent('Non valido.');
  });

  it('Accetta è disabilitato mentre un salvataggio è in corso', () => {
    const registro = registroCon(involucroCrittografia());
    registro.imposta({ stato: 'in-corso', errore: null });
    prepara({ registro });

    expect(screen.getByRole('button', { name: 'Accetta' })).toBeDisabled();
  });
});
