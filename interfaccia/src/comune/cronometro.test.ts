import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { creaCronometro, registraIstante, statoIniziale, tempoAttivo } from './cronometro';

const s = (secondi: number) => secondi * 1000;

describe('tempoAttivo (funzione pura degli istanti)', () => {
  it("l'esempio del progetto dà 160 secondi", () => {
    // Apertura a 0, 60, 150, ricaricamento, 400, 410 (Accetta): intervalli 60, 90, 250 (escluso), 10.
    expect(tempoAttivo([0, 60, 150, 400, 410].map(s))).toBe(s(160));
  });

  it('un intervallo di esattamente 120.000 ms conta', () => {
    expect(tempoAttivo([0, 120_000])).toBe(120_000);
  });

  it('un intervallo di 120.001 ms conta 0', () => {
    expect(tempoAttivo([0, 120_001])).toBe(0);
  });

  it('un intervallo negativo conta 0', () => {
    expect(tempoAttivo([s(100), s(50)])).toBe(0);
  });

  it('senza istanti o con uno solo il tempo è 0', () => {
    expect(tempoAttivo([])).toBe(0);
    expect(tempoAttivo([s(5)])).toBe(0);
  });

  it("dopo un intervallo negativo si riparte dall'istante nuovo", () => {
    expect(tempoAttivo([s(100), s(50), s(60)])).toBe(s(10));
  });
});

describe('ripresa dopo il ricaricamento', () => {
  it('riparte dai valori salvati con la stessa regola dei 2 minuti', () => {
    let stato = statoIniziale(s(150), new Date(s(150)).toISOString());
    stato = registraIstante(stato, s(400));
    stato = registraIstante(stato, s(410));
    expect(stato.tempoAttivoMs).toBe(s(160));
  });

  it('senza valori salvati il primo istante non conta', () => {
    const stato = registraIstante(statoIniziale(0, null), s(10));
    expect(stato).toEqual({ tempoAttivoMs: 0, ultimaAttivitaMs: s(10) });
  });

  it('una data salvata non valida si tratta come assente', () => {
    expect(statoIniziale(5, 'ieri')).toEqual({ tempoAttivoMs: 5, ultimaAttivitaMs: null });
  });
});

describe('creaCronometro con orologio simulato', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-12-01T10:00:00.000Z'));
  });
  afterEach(() => vi.useRealTimers());

  const nuovo = (iniziale = statoIniziale(0, null)) => {
    const invia = vi.fn(() => Promise.resolve());
    const cronometro = creaCronometro({ ora: () => Date.now(), invia, iniziale });
    cronometro.avvia();
    return { cronometro, invia };
  };

  it('invia ogni 15 secondi solo se qualcosa è cambiato', async () => {
    const { cronometro, invia } = nuovo();
    cronometro.attivita('apertura');
    await vi.advanceTimersByTimeAsync(15_000);
    expect(invia).toHaveBeenCalledTimes(1);

    await vi.advanceTimersByTimeAsync(15_000);
    expect(invia).toHaveBeenCalledTimes(1);

    cronometro.attivita('input');
    await vi.advanceTimersByTimeAsync(15_000);
    expect(invia).toHaveBeenCalledTimes(2);
    cronometro.ferma();
  });

  it("l'invio porta tempo attivo e ultima attività in ISO", async () => {
    const { cronometro, invia } = nuovo();
    cronometro.attivita('apertura');
    await vi.advanceTimersByTimeAsync(10_000);
    cronometro.attivita('keydown');
    await vi.advanceTimersByTimeAsync(5_000);
    expect(invia).toHaveBeenCalledWith({ tempoAttivoMs: 10_000, ultimaAttivitaIl: '2026-12-01T10:00:10.000Z' });
    cronometro.ferma();
  });

  it('pointermove conta al massimo una volta ogni 5 secondi', () => {
    const { cronometro } = nuovo();
    cronometro.attivita('apertura');
    vi.advanceTimersByTime(5_000);
    cronometro.attivita('pointermove');
    vi.advanceTimersByTime(1_000);
    cronometro.attivita('pointermove');
    expect(cronometro.misura().tempoAttivoMs).toBe(5_000);
    vi.advanceTimersByTime(4_000);
    cronometro.attivita('pointermove');
    expect(cronometro.misura().tempoAttivoMs).toBe(10_000);
    cronometro.ferma();
  });

  it('una pausa oltre 2 minuti non conta', () => {
    const { cronometro } = nuovo();
    cronometro.attivita('apertura');
    vi.advanceTimersByTime(30_000);
    cronometro.attivita('input');
    vi.advanceTimersByTime(180_000);
    cronometro.attivita('input');
    expect(cronometro.misura().tempoAttivoMs).toBe(30_000);
    cronometro.ferma();
  });

  it("concludi registra l'esito come ultima attività e dopo ferma non si invia più nulla", async () => {
    const { cronometro, invia } = nuovo();
    cronometro.attivita('apertura');
    vi.advanceTimersByTime(7_000);
    const misura = cronometro.concludi();
    expect(misura.tempoAttivoMs).toBe(7_000);
    cronometro.ferma();

    cronometro.attivita('input');
    await vi.advanceTimersByTimeAsync(60_000);
    expect(invia).not.toHaveBeenCalled();
  });

  it('un invio fallito si ritenta al giro successivo', async () => {
    const invia = vi.fn().mockRejectedValueOnce(new Error('rete')).mockResolvedValue(undefined);
    const cronometro = creaCronometro({ ora: () => Date.now(), invia, iniziale: statoIniziale(0, null) });
    cronometro.avvia();
    cronometro.attivita('apertura');
    await vi.advanceTimersByTimeAsync(15_000);
    await vi.advanceTimersByTimeAsync(15_000);
    expect(invia).toHaveBeenCalledTimes(2);
    cronometro.ferma();
  });

  it('dopo il ricaricamento riparte dai valori salvati', () => {
    const { cronometro } = nuovo(statoIniziale(s(150), new Date(Date.now() - s(30)).toISOString()));
    cronometro.attivita('apertura');
    expect(cronometro.misura().tempoAttivoMs).toBe(s(150) + s(30));
    cronometro.ferma();
  });

  it("chiudi (smontaggio) fa un ultimo invio se il tempo è cambiato e l'esito non è stato mandato", async () => {
    const { cronometro, invia } = nuovo();
    cronometro.attivita('apertura');
    vi.advanceTimersByTime(7_000);
    cronometro.attivita('input');

    await cronometro.chiudi();

    expect(invia).toHaveBeenCalledTimes(1);
    expect(invia).toHaveBeenCalledWith({ tempoAttivoMs: 7_000, ultimaAttivitaIl: '2026-12-01T10:00:07.000Z' });
  });

  it("chiudi non manda nulla se non è cambiato niente dall'ultimo invio", async () => {
    const { cronometro, invia } = nuovo();
    cronometro.attivita('apertura');
    await vi.advanceTimersByTimeAsync(15_000);
    expect(invia).toHaveBeenCalledTimes(1);

    await cronometro.chiudi();

    expect(invia).toHaveBeenCalledTimes(1);
  });

  it("chiudi dopo l'esito (ferma già chiamato) non manda nulla", async () => {
    const { cronometro, invia } = nuovo();
    cronometro.attivita('apertura');
    vi.advanceTimersByTime(3_000);
    cronometro.concludi();
    cronometro.ferma();

    await cronometro.chiudi();

    expect(invia).not.toHaveBeenCalled();
  });

  it('inviaOra manda subito se qualcosa è cambiato, ma non ferma il cronometro', async () => {
    const { cronometro, invia } = nuovo();
    cronometro.attivita('apertura');
    vi.advanceTimersByTime(4_000);
    cronometro.attivita('keydown');

    await cronometro.inviaOra();
    vi.advanceTimersByTime(1_000);
    cronometro.attivita('keydown');
    await vi.advanceTimersByTimeAsync(15_000);

    expect(invia).toHaveBeenCalledTimes(2);
    cronometro.ferma();
  });
});
