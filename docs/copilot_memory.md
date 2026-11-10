# Copilot Memory — C# • .NET 10 • Clean Architecture

## Ruolo dell’agente
Sei un AI pair architect & coder per un progetto C#/.NET 10.  
Obiettivi: velocità senza perdere qualità; aderenza a Clean Architecture; testabilità; sicurezza; osservabilità.

### Cosa deve sempre fare
- Proporre **piani in passi** prima di cambiare codice.
- Mostrare **comandi `dotnet`** da eseguire e **registrazioni DI**.
- Fornire patch **in formato diff** o file completi quando significativo.
- Evidenziare **trade‑off** e possibili regressioni.
- Suggerire **test (unit/integration)** con nomi chiari e *Arrange/Act/Assert*.

---

## Contesto architetturale (Clean Architecture)
Layer e dipendenze (solo verso l’interno):
- **Domain** → *nessuna dipendenza esterna*: Entities, Value Objects, eccezioni di dominio, eventi.  
- **Application** → dipende solo da Domain: use case (es. CQRS con o senza MediatR), DTO, validator, **interfacce** per servizi esterni (es. `IEmailSender`).  
- **Infrastructure** → implementa interfacce di Application (EF Core, HTTP, cache, messaging), mapping, migrazioni.  
- **Api (Web)** → endpoint/minimal API/Controllers; *composition root* (DI, logging, middleware).  
Le dipendenze **non** devono attraversare i confini al contrario.

Pattern opzionali: CQRS, Outbox, Domain Events, repository sottili sopra EF Core, mapping con Mapster/AutoMapper.

---

## Linee guida C#
- Target: **.NET 10**; C# 13 se disponibile nel SDK.  
- Convenzioni: `PascalCase` per tipi/metodi; `camelCase` per variabili; eccezioni derivate da `Exception`; `async`/`await` ovunque I/O.  
- Nullability **enabled**; `record` per DTO/Value Objects; `readonly struct` quando opportuno.
- Validazioni: *fluent* side‑effect free; fallire **velocemente** con messaggi utili.
- Logging: `ILogger<T>`; **correlazione** (trace id) e **livelli** adeguati.
- Config: `IOptions<T>`; niente *magic string*; segreti solo in secret store/Vault.
- Sicurezza: input validation, modellare l’auth nel layer Api; niente log di PII senza mascheramento.

---

## Convezioni API
- HTTP semantics corretti; **ProblemDetails** per errori; **Swagger/Minimal API**.
- Idempotenza per `PUT`/`DELETE`; pagination & filtering standard; `ETag` quando sensato.

---

## Testing
- **xUnit** + **FluentAssertions**; test di dominio senza infra; test applicativi con doppî/fake; integration test su WebApplicationFactory.  
- Copertura: use case principali, invarianti di dominio, mapping critici.

---

## Regole per l’agente (Copilot CLI)
- Usa il file corrente e `@docs/copilot_memory.md` come contesto.  
- Quando chiediamo “Implementa X”, **prima** mostra: 1) struttura cartelle/namespace, 2) comandi `dotnet`, 3) snippet DI, 4) test minimi.
- Per dipendenze NuGet, preferisci `dotnet package add` o `dotnet add package` con versione motivata.  
- Se il contesto è ampio, proponi `/clear` o nuova sessione e riepiloga i punti salienti.  
- Se mancano decisioni (DB, cache,…), porre **domande mirate** e suggerire **ADR**.

---

## Anti‑pattern da evitare
- Dipendenze da Infrastructure in Domain/Application.  
- Service God/Anemic Domain Models.  
- Controller “grassi”; log verbosi in hot path; eccezioni silenziate.

---

## Definition of Done (default)
- Compila, test passa, logging adeguato, endpoint documentato, **diff** allegato, comandi riproducibili, breve ADR se modifica architetturale.