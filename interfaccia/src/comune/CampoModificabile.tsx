import { useId } from 'react';

interface CampoModificabileProps {
  etichetta: string;
  valore: string | null;
  onCambia: (valore: string) => void;
  /** Numero di righe: oltre 1 il campo è un'area di testo. */
  righe?: number;
  descrizione?: string;
  solaLettura?: boolean;
}

export function CampoModificabile({ etichetta, valore, onCambia, righe = 1, descrizione, solaLettura = false }: CampoModificabileProps) {
  const id = useId();
  const descrizioneId = descrizione ? `${id}-d` : undefined;
  const comune = {
    id,
    value: valore ?? '',
    readOnly: solaLettura,
    'aria-describedby': descrizioneId,
    onChange: (e: { target: { value: string } }) => onCambia(e.target.value),
  };

  return (
    <div className="campo">
      <label htmlFor={id}>{etichetta}</label>
      {righe > 1 ? <textarea rows={righe} {...comune} /> : <input type="text" {...comune} />}
      {descrizione ? (
        <p id={descrizioneId} className="campo-descrizione">
          {descrizione}
        </p>
      ) : null}
    </div>
  );
}
