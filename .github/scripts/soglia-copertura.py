#!/usr/bin/env python3
"""Soglia di copertura delle righe per Domain, Application e Tipologie (ROADMAP, fase 0, criterio 11).

Uso: soglia-copertura.py <cartella-dei-risultati> [soglia-percentuale]
Scrive il riepilogo in $GITHUB_STEP_SUMMARY se presente; esce con 1 sotto la soglia.
"""
import glob
import os
import sys
import xml.etree.ElementTree as ET

PACCHETTI = ("DBEnigmistica.Domain", "DBEnigmistica.Application", "DBEnigmistica.Tipologie")


def righe_per_pacchetto(file_report):
    """Restituisce {pacchetto: {(file, riga): massimo dei passaggi}} da un report Cobertura."""
    risultato = {p: {} for p in PACCHETTI}
    for pacchetto in ET.parse(file_report).getroot().iter("package"):
        nome = pacchetto.get("name")
        if nome not in risultato:
            continue
        for classe in pacchetto.iter("class"):
            sorgente = classe.get("filename", "")
            for riga in classe.iter("line"):
                chiave = (sorgente, riga.get("number"))
                passaggi = int(riga.get("hits", "0"))
                risultato[nome][chiave] = max(risultato[nome].get(chiave, 0), passaggi)
    return risultato


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    cartella = sys.argv[1]
    soglia = float(sys.argv[2]) if len(sys.argv) > 2 else 80.0

    report = glob.glob(os.path.join(cartella, "**", "coverage.cobertura.xml"), recursive=True)
    if not report:
        print("Nessun report di copertura in " + cartella)
        return 1

    uniti = {p: {} for p in PACCHETTI}
    for f in report:
        for pacchetto, righe in righe_per_pacchetto(f).items():
            for chiave, passaggi in righe.items():
                uniti[pacchetto][chiave] = max(uniti[pacchetto].get(chiave, 0), passaggi)

    totali = coperte = 0
    tabella = ["| Pacchetto | Righe coperte | Righe totali | Copertura |", "|---|---|---|---|"]
    for pacchetto in PACCHETTI:
        t = len(uniti[pacchetto])
        c = sum(1 for p in uniti[pacchetto].values() if p > 0)
        totali += t
        coperte += c
        tabella.append("| {} | {} | {} | {:.1f}% |".format(pacchetto, c, t, 100.0 * c / t if t else 0.0))

    if totali == 0:
        print("Nessuna riga trovata per i pacchetti " + ", ".join(PACCHETTI))
        return 1

    percentuale = 100.0 * coperte / totali
    esito = "superata" if percentuale >= soglia else "NON superata"
    righe_testo = ["## Soglia di copertura", ""] + tabella + [
        "",
        "Insieme: **{:.1f}%** ({} su {}); soglia {:.0f}%: {}.".format(percentuale, coperte, totali, soglia, esito),
    ]
    testo = "\n".join(righe_testo)
    print(testo)

    riepilogo = os.environ.get("GITHUB_STEP_SUMMARY")
    if riepilogo:
        with open(riepilogo, "a", encoding="utf-8") as f:
            f.write(testo + "\n")

    if percentuale < soglia:
        print("::error::Copertura delle righe di Domain e Application sotto la soglia: {:.1f}% < {:.0f}%.".format(percentuale, soglia))
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
