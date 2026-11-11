#!/usr/bin/env bash
# Uso: verifica-tag-da-main.sh <sha-del-tag> <ramo-principale>
# Esce con 0 solo se il commit del tag e' contenuto nella storia del ramo principale.
set -euo pipefail

sha="${1:?Uso: $0 <sha> <ramo-principale>}"
main="${2:?Uso: $0 <sha> <ramo-principale>}"

if ! git rev-parse --verify --quiet "${main}^{commit}" >/dev/null; then
  echo "::error::Il ramo principale '${main}' non esiste nel repository clonato."
  exit 2
fi

if git merge-base --is-ancestor "$sha" "$main"; then
  echo "Il commit ${sha} appartiene a ${main}: tag valido."
else
  echo "::error::Il commit ${sha} non appartiene a ${main}: un tag di versione deve puntare a un commit del ramo principale."
  exit 1
fi
