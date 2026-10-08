# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SquadComplete is the monorepo behind **Ultimate 11: Squad Draft**, a daily football (soccer) drafting game. It has four projects plus a shared SQL directory:

- `squad-draft/` — React 19 + TypeScript + Vite SPA (client), deployed to Azure Static Web Apps.
- `squad-api/` — .NET 8 Minimal API backend (EF Core + PostgreSQL/Npgsql), with Scalar/OpenAPI docs.
- `squad-func/` — Azure Functions (isolated worker, .NET 8) for cron jobs, historical match ingestion, Gemini AI searches, and blob processing.
- `squad-domain/` — Shared C# class library with EF Core entity models used by both `squad-api` and `squad-func`.
- `sql/` — Database schema, seed scripts, and migrations.

## Build Commands

From the repo root:
```bash
make all           # builds squad-api, squad-draft, squad-func
make build-api      # dotnet build squad-api
make build-draft     # npm run build in squad-draft
make build-func      # dotnet build squad-func
make clean          # removes squad-draft/dist, node_modules, and dotnet clean on api/func
make trim-branches  # deletes local git branches already merged (excludes master/main/dev)
```

Per-project (run from inside the directory unless noted):
```bash
# squad-draft
npm run dev       # local dev server
npm run build     # tsc + vite build (type-check + bundle)
npm run preview   # preview production build

# squad-api
dotnet build squad-api
dotnet run --project squad-api      # local API instance; requests collection in squad-api/squad-api.http

# squad-func
dotnet build squad-func
func start --prefix squad-func      # requires Azure Functions Core Tools
```

There is no automated test suite in this repo currently — "clean build" is the verification bar (`npm run build`, `dotnet build`).

## Mandatory Workflow Rules (from AGENTS.md)

These are strictly enforced repo-wide conventions — follow them for every change:

1. **Changelog Maintenance**: After any feature change, bug fix, or merge to `main`, update `CHANGELOG.md` in every affected subproject (`squad-draft/`, `squad-api/`, `squad-func/`), plus the root `CHANGELOG.md` for cross-cutting changes. Use Keep a Changelog categories (`Added`, `Changed`, `Fixed`, `Removed`).
2. **Clean Build Gate**: Verify the affected subproject builds cleanly before finalizing (see Build Commands above).
3. **Zero Secret Leakage**: Never hardcode API keys, connection strings, or tokens. Use `squad-draft/.env.local`, `squad-api/appsettings.Development.json`, or `squad-func/local.settings.json`.

Specialist personas are defined under `.agents/` (`code-reviewer.md`, `reactjs-specialist.md`, `dotnet-specialist.md`) with matching skills in `.agents/skills/` — adopt the relevant persona's checklist when doing focused frontend/backend/review work.

## Architecture Notes

### squad-draft (frontend)
- Flat layout at the project root (no `src/`): `App.tsx` is the main component (~35KB, holds most orchestration logic), with `api.ts`, `constants.tsx`, `types.ts`, `index.tsx`, and a `components/` directory (`Pitch.tsx`, `PlayerCard.tsx`, `TeamRoster.tsx`, `Leaderboard.tsx`, `FixtureInfo.tsx`, `AboutDialog.tsx`, `AlertDialog.tsx`, `CookieConsent.tsx`, `MaterialDatePicker.tsx`).
- Daily draft flow: `fetchDailySquads()` in `api.ts` loads the day's challenge/formation from the Functions host (`VITE_FUNCTIONS_BASE_URL`, as do `fetchFixture` and `recordRequest`; everything else uses `VITE_API_BASE_URL`); draft state (`currentStep` 0–11, `selectedPlayers`, `formation`, `completed`, `submitted`) persists to `localStorage` under `squad-draft-${YYYY-MM-DD}`.
- Two-step player placement: click a player, then click a formation spot (`tempPlayer`/`activeSpotId`); `Pitch.tsx` also supports native HTML5 drag-and-drop onto formation spots positioned with percentage-based `top`/`left`.
- Submission payload includes `BrowserIdentifierId` (`squad-browser-id`), `UserName`, `GameRecordId`, `FormationId`, `Players`; the backend deduplicates daily submissions by browser ID / game record.
- All domain interfaces live in `types.ts` (`Player`, `Squad`, `FormationSpot`, `DraftState`, `Position`) — strict typing, no `any`.

### squad-api (backend)
- `Program.cs`: DI setup, EF Core `SquadContext` registration (Npgsql; the context class itself lives in `squad-domain`), CORS (allow-any policy), JSON `ReferenceHandler.IgnoreCycles` for circular EF navigation properties, OpenAPI + Scalar.
- `Endpoints/`: one file per resource (e.g. `GameRecordEndpoints.cs`, `PlayerEndpoints.cs`, `LeagueEndpoints.cs`), all wired together via `MapAllEndpoints()` in `EndpointExtensions.cs`.
- `Services/`: business logic injected as scoped services (e.g. `GameRecordService.cs`) — endpoint handlers stay thin (parse input → delegate to service → return typed `Results.*`).
- `DTOs/`: request/response contracts, kept separate from the EF entities in `squad-domain`.
- Convention: async EF Core calls everywhere, `.AsNoTracking()` for reads, explicit `.Include()`/`.ThenInclude()` to avoid N+1s.

### squad-func (background jobs)
- Isolated worker model with constructor-injected `HttpClientFactory`, `ILogger<T>`, and `SquadContext`.
- Key functions: `SquadSelector.cs` (schedules daily squads/formations), `SingleMatchHistoricalSearch.cs`/`FullSeasonAISearch.cs` (Gemini-driven curation of historic matches), `GenerateFixtureFromAIMatchData.cs` (parses Gemini-generated blob data into fixtures/lineups/players), `TeamRefresh.cs` (syncs lineups/metadata from external sports APIs), `CleanupGameRecords.cs` (purges stale/test records), `RecordRequest.cs` (HTTP-triggered engagement tracking), `GetFixture.cs`/`GetGameRecordByDate.cs` (anonymous HTTP triggers that mirror `squad-api` routes `/api/fixtures/{id}` and `/api/game-records/date/{date}` so the client can skip the API's cold start — keep their JSON shapes in sync with the API), `GetPlayerImage.cs`/`DailyReport.cs`.
- `Services/`: `GeminiService.cs` (Gemini AI calls), `ApiService.cs` (external sports API client), `StorageService.cs` (blob storage), `EmailSMTPService.cs`, `LoggingHandler.cs`.
- `prompts/`: Gemini prompt templates (`agent-prompt.md`, `team_fixture_prompt.md`, `playername-prompt.md`, `history.md`) used by the AI search/ingestion functions.
- Blob-triggered processors must archive/delete processed blobs to avoid reprocessing loops; external API calls should tolerate rate limiting from Gemini and sports data providers.

### squad-domain (shared models)
- The single `SquadContext` (DbSets, indexes, precision) lives in `squad-domain/Models/SquadContext.cs`; `squad-api` and `squad-func` both register it — do not re-declare a context in either host.
- Plain EF Core entity classes shared by `squad-api` and `squad-func` (`Player`, `Team`, `Fixture`, `Season`, `TeamSeason`, `League`, `Formation`, `GameRecord`, `GameRecordTag`, `UserSquad`, `UserSquadPlayer`, `Event`, `PlayerFixtureStatistic`, `User`, `Feedback`). Both other projects reference this library rather than duplicating models — add new persisted entities here, not in `squad-api/Models` or `squad-func/Models`.
