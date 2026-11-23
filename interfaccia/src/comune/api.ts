import type { Candidato, EsitoGenerazione, Involucro, MisuraTempo, RichiestaAccetta, RichiestaScarto, Tipologia } from './tipi';

export class ErroreApi extends Error {
  constructor(
    message: string,
    readonly stato: number,
    readonly errori: string[],
  ) {
    super(message);
    this.name = 'ErroreApi';
  }
}

type Fetch = typeof fetch;

/** Servizi di revisione usati dalle schermate: C e A li ricevono per parametro e si provano con un finto. */
export interface Api {
  tipologie(): Promise<Tipologia[]>;
  genera(brief: unknown): Promise<EsitoGenerazione>;
  prossimo(tipologia: string): Promise<Candidato | null>;
  salvaFinale(id: string, contenuto: Involucro): Promise<Candidato>;
  salvaTempo(id: string, misura: MisuraTempo): Promise<Candidato>;
  accetta(id: string, richiesta: RichiestaAccetta): Promise<Candidato>;
  scarta(id: string, richiesta: RichiestaScarto): Promise<Candidato>;
}

const RADICE = '/api/prototipo';

/** Testo da mostrare per un errore qualsiasi dell'API o della rete. */
export function messaggioDiErrore(errore: unknown): string {
  if (errore instanceof ErroreApi) return errore.errori.length > 0 ? errore.errori.join(' ') : errore.message;
  return errore instanceof Error ? errore.message : 'Errore sconosciuto.';
}

function leggiJson(testo: string): unknown {
  try {
    return testo.length > 0 ? JSON.parse(testo) : null;
  } catch {
    return null;
  }
}

export function creaApi(fetchImpl: Fetch = (...argomenti) => fetch(...argomenti)): Api {
  async function chiama<T>(metodo: string, percorso: string, corpo?: unknown, keepalive = false): Promise<T | null> {
    let risposta: Response;
    try {
      risposta = await fetchImpl(`${RADICE}${percorso}`, {
        method: metodo,
        headers: { 'Content-Type': 'application/json' },
        body: corpo === undefined ? undefined : JSON.stringify(corpo),
        ...(keepalive ? { keepalive: true } : {}),
      });
    } catch {
      throw new ErroreApi('Il prototipo non risponde: controlla che il comando dbe prototipo sia in esecuzione.', 0, []);
    }

    if (risposta.status === 204) return null;
    const testo = await risposta.text();
    const json = leggiJson(testo);

    if (!risposta.ok) {
      const errori = (json as { errori?: unknown } | null)?.errori;
      const elenco = Array.isArray(errori) ? errori.filter((e): e is string => typeof e === 'string') : [];
      throw new ErroreApi(`Richiesta non riuscita (codice ${risposta.status}).`, risposta.status, elenco);
    }

    return json as T;
  }

  const richiesta = async <T>(metodo: string, percorso: string, corpo?: unknown, keepalive = false): Promise<T> =>
    (await chiama<T>(metodo, percorso, corpo, keepalive)) as T;

  return {
    tipologie: async () => (await richiesta<{ tipologie: Tipologia[] }>('GET', '/tipologie')).tipologie,
    genera: (brief) => richiesta<EsitoGenerazione>('POST', '/candidati', brief),
    prossimo: (tipologia) => chiama<Candidato>('POST', `/tipologie/${encodeURIComponent(tipologia)}/prossimo`, {}),
    salvaFinale: (id, contenuto) => richiesta<Candidato>('PUT', `/candidati/${id}/finale`, contenuto),
    // keepalive: l'ultimo invio alla chiusura della pagina non deve essere annullato dal browser.
    salvaTempo: (id, misura) => richiesta<Candidato>('PUT', `/candidati/${id}/tempo`, misura, true),
    accetta: (id, r) => richiesta<Candidato>('POST', `/candidati/${id}/accetta`, r),
    scarta: (id, r) => richiesta<Candidato>('POST', `/candidati/${id}/scarta`, r),
  };
}
