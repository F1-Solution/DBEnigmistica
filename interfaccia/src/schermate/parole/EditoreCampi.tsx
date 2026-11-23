import type { ReactNode } from 'react';
import { CampoModificabile } from '../../comune/CampoModificabile';
import { ElencoControlli } from '../../comune/ElencoControlli';
import { AvvisoSalvataggio } from '../../comune/AvvisoSalvataggio';
import type { Api } from '../../comune/api';
import type { Candidato } from '../../comune/tipi';
import { conDati, useSalvataggio } from '../../comune/useSalvataggio';

export interface CampoTesto {
  nome: string;
  etichetta: string;
  righe?: number;
  /** I campi facoltativi vuoti si salvano come assenti (null). */
  facoltativo?: boolean;
  descrizione?: string;
}

/** Campi modificabili di crittografia breve e anagramma. */
export const CAMPI_PAROLE: Readonly<Record<string, CampoTesto[]>> = {
  'crittografia-breve': [
    { nome: 'esposto', etichetta: 'Esposto', righe: 3 },
    { nome: 'diagramma', etichetta: 'Diagramma', descrizione: 'Senza parentesi, per esempio 5 2 = 7.' },
    { nome: 'prima_lettura', etichetta: 'Prima lettura', righe: 2 },
    { nome: 'soluzione', etichetta: 'Soluzione', righe: 2 },
    { nome: 'spiegazione', etichetta: 'Spiegazione', righe: 3, facoltativo: true },
  ],
  anagramma: [
    { nome: 'esposto', etichetta: 'Esposto', righe: 5 },
    { nome: 'diagramma', etichetta: 'Diagramma', descrizione: 'Senza parentesi, per esempio 4 = 4.' },
    { nome: 'prima_parola', etichetta: 'Prima parola', facoltativo: true },
    { nome: 'soluzione', etichetta: 'Soluzione' },
    { nome: 'autore', etichetta: 'Autore', facoltativo: true },
  ],
};

interface EditoreCampiProps {
  candidato: Candidato;
  servizi: Pick<Api, 'salvaFinale'>;
  children?: ReactNode;
}

function testo(valore: unknown): string | null {
  return typeof valore === 'string' ? valore : null;
}

export function EditoreCampi({ candidato, servizi, children }: EditoreCampiProps) {
  const { contenuto, controlli, stato, errore, modifica } = useSalvataggio(candidato, servizi);
  const campi = CAMPI_PAROLE[candidato.tipologia] ?? [];
  const sottotipo = testo(contenuto.dati['sottotipo']);

  return (
    <div className="editore">
      {sottotipo ? <p className="sottotipo">Sottotipo: {sottotipo}</p> : null}
      {campi.map((campo) => (
        <CampoModificabile
          key={campo.nome}
          etichetta={campo.etichetta}
          righe={campo.righe}
          descrizione={campo.descrizione}
          valore={testo(contenuto.dati[campo.nome])}
          onCambia={(v) => modifica((c) => conDati(c, { [campo.nome]: campo.facoltativo && v === '' ? null : v }))}
        />
      ))}
      <AvvisoSalvataggio stato={stato} errore={errore} />
      <AvvisiDellAutore avvisi={contenuto.avvisi} />
      <ElencoControlli controlli={controlli} />
      {children}
    </div>
  );
}

export function AvvisiDellAutore({ avvisi }: { avvisi: readonly string[] }) {
  if (avvisi.length === 0) return null;
  return (
    <section aria-label="Dubbi dell'autore" className="avvisi">
      <h3>Dubbi dell&apos;autore</h3>
      <ul>
        {avvisi.map((a, i) => (
          <li key={`${i}-${a}`}>{a}</li>
        ))}
      </ul>
    </section>
  );
}
