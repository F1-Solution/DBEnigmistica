# Passi Node per `ci.yml` e `publish.yml` (ondata 1b.2, agente A)

Per B. Versioni e SHA delle azioni come già in uso nei flussi (`actions/setup-node` v7.1.0). Node 24 è in `interfaccia/.nvmrc`.

## `ci.yml`, lavoro "Build e test", prima della compilazione .NET

```yaml
      - name: Configura Node
        uses: actions/setup-node@949feb2413d6458794dcd2491c4babbbce0c15c1 # v7.1.0
        with:
          node-version-file: interfaccia/.nvmrc
          cache: npm
          cache-dependency-path: interfaccia/package-lock.json

      - name: Installa i pacchetti dell'interfaccia
        working-directory: interfaccia
        run: npm ci

      - name: Interfaccia - lint
        working-directory: interfaccia
        run: npm run lint

      - name: Interfaccia - controllo dei tipi
        working-directory: interfaccia
        run: npm run typecheck

      - name: Interfaccia - test
        working-directory: interfaccia
        run: npm test

      - name: Interfaccia - build
        working-directory: interfaccia
        run: npm run build
```

Poi la compilazione .NET con `-p:RichiediInterfaccia=true` (la build fallisce se manca `interfaccia/dist/index.html`):

```yaml
        run: dotnet build DBEnigmistica.slnx --configuration Release --no-restore -p:RichiediInterfaccia=true
```

La pubblicazione di controllo `win-x64` di `ci.yml` aggiunge lo stesso `-p:RichiediInterfaccia=true` e, dopo `dotnet publish`, il controllo:

```yaml
      - name: Controlla che l'archivio contenga l'interfaccia
        run: test -f "<cartella-di-pubblicazione>/wwwroot/index.html"
```

## `ci.yml`, lavoro "Vulnerabilita pacchetti"

```yaml
      - name: Configura Node
        uses: actions/setup-node@949feb2413d6458794dcd2491c4babbbce0c15c1 # v7.1.0
        with:
          node-version-file: interfaccia/.nvmrc

      - name: Vulnerabilita dei pacchetti npm
        working-directory: interfaccia
        run: npm audit --audit-level=high
```

Stato verificato il 23/11/2026: 0 vulnerabilita alte o critiche, 8 moderate (catena di `minimatch` dentro ESLint e typescript-eslint, solo sviluppo); il controllo con `high` esce con 0.

## `publish.yml`, prima di `dotnet publish` del `win-x64`

Gli stessi passi "Configura Node", "Installa i pacchetti dell'interfaccia" e "Interfaccia - build" (con `npm ci` e `npm run build`), poi `dotnet publish ... -p:RichiediInterfaccia=true` e dopo la pubblicazione il passo `test -f .../wwwroot/index.html`. Le pre-release non spostano `latest`: invariato.

## `dependabot.yml`

```yaml
  - package-ecosystem: "npm"
    directory: "/interfaccia"
    schedule:
      interval: "weekly"
```

## Note

- `interfaccia/dist/` e `node_modules/` sono in `.gitignore`.
- `npm ci` richiede `interfaccia/package-lock.json`, tracciato.
- I test Vitest durano circa 15 secondi, la build meno di un secondo; `npm ci` e il passo piu lento.
