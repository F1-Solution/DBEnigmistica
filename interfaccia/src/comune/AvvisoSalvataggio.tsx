import type { StatoSalvataggio } from './useSalvataggio';

interface AvvisoSalvataggioProps {
  stato: StatoSalvataggio;
  errore: string | null;
}

export function AvvisoSalvataggio({ stato, errore }: AvvisoSalvataggioProps) {
  if (stato === 'errore') {
    return (
      <p role="alert" className="errore">
        Salvataggio non riuscito: {errore ?? 'errore sconosciuto.'}
      </p>
    );
  }

  return (
    <p role="status" className="stato-salvataggio">
      {stato === 'in-corso' ? 'Salvataggio in corso…' : 'Modifiche salvate.'}
    </p>
  );
}
