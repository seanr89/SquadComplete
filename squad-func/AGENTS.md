# AGENTS.md — squad-func (Azure Functions Background Service)

This file contains scoped instructions and guidelines for AI agents working within the `squad-func` directory.

---

## 1. Technical Stack & Environment

- **Runtime**: Azure Functions (.NET 8 Isolated Worker model)
- **Language**: C# 12
- **Triggers**: Timer Triggers (CRON), and HTTP Triggers
- **External Integrations**:
  - Google Gemini AI API for historical football fixture analysis
  - Azure Blob Storage (`playersname` container only) for player images
  - Sports Data APIs for real player profile and stat enrichment
  - PostgreSQL database via Entity Framework Core

---

## 2. Core Functions & Responsibilities

- **`SquadSelector.cs`**: Selects and schedules the daily squads and formations for upcoming game records.
- **`CleanupGameRecords.cs`**: Maintenance cron function purging stale or test records.
- **`RecordRequest.cs`**: Lightweight HTTP trigger tracking player engagement and session analytics.
- **`GetFixture.cs` & `GetGameRecordByDate.cs`**: Anonymous read-only HTTP triggers mirroring `squad-api` routes (`/api/fixtures/{id}`, `/api/game-records/date/{date}`) so the client avoids the API's cold start. Keep their JSON shapes identical to the API's (`Models/GameRecordDto.cs` copies the API's DTOs).

---

## 3. Engineering Conventions & Best Practices

- **Isolated Worker Model**:
  - Use constructor dependency injection for `HttpClientFactory`, `ILogger<T>`, and database contexts.
  - Return typed function outputs and use binding attributes (`[TimerTrigger]`, `[HttpTrigger]`).
- **Resilience & Fault Tolerance**:
  - Handle rate limits from external APIs (Gemini AI and sports APIs) with retry policies or graceful fallback logic.
  - Only the `playersname` blob container is accessed (read-only, player images); don't add other container calls.
- **Secrets & Configuration**:
  - Use `local.settings.json` for local development. Never commit sensitive values.
  - Target Azure Key Vault or Function App AppSettings in production.

---

## 4. Verification Commands

Run from the repository root or within `squad-func/`:
```bash
# Build the Azure Functions project
dotnet build squad-func

# Run functions host locally (requires Azure Functions Core Tools)
func start --prefix squad-func
```

---

## 5. Changelog Requirement

Every change in `squad-func` **must** be documented in `squad-func/CHANGELOG.md` under the appropriate section before closing out work.
