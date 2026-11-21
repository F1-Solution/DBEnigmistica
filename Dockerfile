# syntax=docker/dockerfile:1
# Immagine dell'applicazione console. Contesto di build: la radice del repository.
# Un Dockerfile nella radice permette di copiare tutti i progetti di src\ (e un futuro
# Directory.Build.props); le immagini di artefatti futuri useranno Dockerfile.<nome>.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=0.0.0
WORKDIR /src
COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/DBEnigmistica.Console/DBEnigmistica.Console.csproj \
      --configuration Release \
      --runtime linux-x64 \
      --self-contained false \
      --output /app \
      -p:Version=${VERSION} \
      -p:UseAppHost=false \
      -p:IncludePrototipo=false
# La pubblicazione deve contenere la configurazione di base.
RUN test -f /app/appsettings.json
# Elenco ammesso delle librerie native: solo PDFium per linux-x64. Qualsiasi altro file nativo, per nome o per primi byte ELF, fa fallire la build.
RUN native="$( { find /app \( -name '*.so' -o -name '*.so.*' -o -name '*.dylib' -o -name 'pdfium.dll' -o -path '*/runtimes/*/native' -o -path '*/runtimes/*/native/*' \); \
                 find /app -type f | while read -r f; do [ "$(head -c4 "$f" | od -An -tx1 | tr -d ' \n')" = "7f454c46" ] && echo "$f"; done; } | sort -u)"; \
    allowed="/app/libpdfium.so"; \
    if [ "$native" != "$allowed" ]; then \
      echo "Librerie native non ammesse nella pubblicazione (ammessa solo $allowed). Trovate:"; echo "$native"; exit 1; \
    fi

# Immagine "chiseled": senza shell ne' gestore pacchetti, utente non root predefinito.
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app .
# La configurazione arriva da variabili d'ambiente (nessun segreto nell'immagine), per esempio:
#   ConnectionStrings__DefaultConnection, Archivio__Cartella, FornitoreAi__Tipo
# CLAUDE_CODE_OAUTH_TOKEN non si usa in fase 1: l'estrazione dal vivo gira su Windows con il login di Claude Code (ADR 010).
USER $APP_UID
ENTRYPOINT ["dotnet", "dbe.dll"]
