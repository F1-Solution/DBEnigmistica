interface ElencoControlliProps {
  controlli: readonly string[];
  /** Controlli del candidato salvato: aiuti per chi rivede, non bloccano Accetta. */
  titolo?: string;
}

export function ElencoControlli({ controlli, titolo = 'Controlli' }: ElencoControlliProps) {
  return (
    <section aria-labelledby="titolo-controlli" className="controlli">
      <h3 id="titolo-controlli">{titolo}</h3>
      {controlli.length === 0 ? (
        <p className="controlli-vuoti">Nessun problema segnalato.</p>
      ) : (
        <ul>
          {controlli.map((controllo, i) => (
            <li key={`${i}-${controllo}`}>{controllo}</li>
          ))}
        </ul>
      )}
    </section>
  );
}
