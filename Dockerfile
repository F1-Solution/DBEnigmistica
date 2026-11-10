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
      --output /app \
      -p:Version=${VERSION} \
      -p:UseAppHost=false
# La pubblicazione deve contenere la configurazione di base.
RUN test -f /app/appsettings.json

# Immagine "chiseled": senza shell ne' gestore pacchetti, utente non root predefinito.
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app .
# La configurazione arriva da variabili d'ambiente (nessun segreto nell'immagine), per esempio:
#   ConnectionStrings__DefaultConnection, Anthropic__ApiKey
USER $APP_UID
ENTRYPOINT ["dotnet", "dbe.dll"]
