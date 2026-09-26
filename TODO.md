# TODO — SquadComplete Review Findings

Generated from a full-codebase review of `squad-draft`, `squad-api`, `squad-func`, and `squad-domain` (2026-09-26). Items are grouped by category; each entry cites the file(s) it applies to. Not exhaustive — prioritized for impact.

## 🔴 Security (do these first)

- [x] **Rotate/verify committed DB credentials** — `squad-api/appsettings.json` has a live-looking Supabase hostname + user id (`postgres.dbplrebihjgnogujbsel@aws-1-eu-west-3.pooler.supabase.com`) committed to git history, password field currently reads literally `"password"`. Confirm whether a real password was ever committed in an earlier revision (`git log -p -- squad-api/appsettings.json`), rotate the DB password regardless since the host/user are now public, and move the real value to `appsettings.Development.json` / environment variables only. Verified: real password never committed (always the literal placeholder). `appsettings.json`'s `ConnectionStrings:DefaultConnection` is now blanked out since Azure App Service already overrides it via `ConnectionStrings__DefaultConnection`. **Still needs manual action from you**: rotate the actual Supabase DB password (dashboard) and update the Azure App Service / Function App settings with the new value — I can't do this part, it needs your Supabase/Azure account access.
- [ ] **`squad-func/local.settings.json` has a real plaintext Supabase password on disk** — not committed (correctly gitignored), but rotate it and confirm it never leaked into an earlier commit. Consider a separate local/dev-only database instead of pointing local dev at the shared instance.
- [x] **Lock down CORS** — `squad-api/Program.cs` uses `AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()`. Restrict to the known frontend origin(s). Fixed: CORS policy now reads an allowlist from `Cors:AllowedOrigins` config, set to `https://squadup.raffrock.com` and `https://blue-wave-059115703.4.azurestaticapps.net` in production and `http://localhost:3000` in Development.
- [x] **No authentication/authorization on any `squad-api` endpoint** — every POST/PUT/DELETE (players, leagues, fixtures, formations, game records, statistics) is open to anyone; add at minimum an API key / admin auth for write endpoints not meant to be public. Fixed: added `AdminApiKeyFilter` (`X-Api-Key` header checked against `Auth:AdminApiKey`, fail-closed) on all admin `POST`/`PUT`/`DELETE` endpoints plus `GET /api/events`; public gameplay endpoints stay open.
- [x] **Mass-assignment risk**: several endpoints bind the raw EF entity from the request body (`PlayerEndpoints.cs`, `LeagueEndpoints.cs`, `FixtureEndpoints.cs`, `FormationEndpoints.cs`, `PlayerFixtureStatisticEndpoints.cs`) instead of a DTO — callers can set `Id`, `CreatedAt`, etc. directly. Introduce DTOs consistently (as already done for `GameRecord`/`Feedback`/`Event`/`UserSquad`). Fixed: added `PlayerDto`, `LeagueDto`, `FixtureDto`, `FormationDto`, `PlayerFixtureStatisticDto` and switched all five endpoints' `POST`/`PUT` handlers to bind them instead of the raw entities.
- [x] **No rate limiting on public write endpoints** — `FeedbackEndpoints.cs`, `EventEndpoints.cs`, `UserSquadEndpoints.cs` (API) and `RecordRequest.cs` (Functions, `AuthorizationLevel.Anonymous`) can be spammed with no throttling or payload size caps. Per-IP fixed-window rate limiting (10 req/min) added to all four; payload size caps still outstanding.
- [x] **`RecordRequest` trusts client-asserted IP/timestamp** — `squad-func/RecordRequest.cs` stores `data.IpAddress`/`data.DateTime` from the request body as-is, defeating the point of an audit/analytics log. Derive these server-side. Fixed: IP now comes from the right-most `X-Forwarded-For` entry (port stripped), falling back to the remote address, and the timestamp is `DateTime.UtcNow`; body values are ignored and the client no longer sends them (ipify call removed).
- [x] **AI-generated content written to DB with minimal validation** — `squad-func/GenerateFixtureFromAIMatchData.cs` inserts Gemini-produced names/ratings with only null-coalescing to `"N/A"`; no length/charset checks despite `[StringLength(255)]` constraints in `squad-domain`, which can throw at `SaveChanges` on unexpectedly long AI output. Fixed: new `Utils/AiDataSanitizer` cleans and truncates all AI text to column limits, clamps and rounds ratings, and strictly parses the score. Invalid payloads (missing teams or competition, bad date or score, same team on both sides) are archived to `archive-invalid-data`. `GetMatchDate` now returns null instead of `MinValue`.
- [x] **CookieConsent banner is decorative** — `squad-draft/components/CookieConsent.tsx` collects analytics/marketing consent that nothing in the app actually checks (`checkConsent('analytics')` is never called before `recordRequest`), so consent is currently non-functional and the tracking call fires regardless.

## 🐞 Bugs

### squad-draft
- [ ] `isDraftComplete` (`App.tsx:117`) is computed but never used to gate completion — only `draft.completed` (set when `selectedPlayers` reaches exactly 11) does. If the API ever returns fewer than 11 daily squads, users get stuck and can never reach submission.
- [x] Formation refresh can silently misassign placed players if the daily formation's spot ids shift shape between fetches (`App.tsx:78-84`). Fixed: `remapFormationPlayers` (`constants.tsx`) matches placed players by line and slot number instead of spot ID. If the new shape can't hold them, the saved layout and `formationId` are kept.
- [ ] `executeResetDraft` (`App.tsx:290-302`) doesn't preserve/clear `gameRecordId`/`formationId` — after a reset these remain `undefined` for the rest of the session since the fetch effect only runs once (`[]` deps).
- [ ] `submitUserSquad` coerces a missing player id to `0` (`parseInt(spot.player?.id || '0', 10)`, `App.tsx:363`) instead of validating all 11 spots are filled before submit.
- [ ] "Advancing to pick number X of 11" message (`App.tsx:275`) relies on a stale-closure value of `draft.currentStep` rather than the freshly computed next step — works today but is fragile.
- [ ] `localStorage.setItem` failures (e.g. `QuotaExceededError`) are only logged, not surfaced to the user — draft silently stops persisting (`App.tsx:104-111`).

### squad-api
- [ ] `GetGameRecordByDateAsync` filters `IsSubstitute == false` but `GetAllGameRecordsAsync`/`GetGameRecordByIdAsync` don't — the same game record returns different player sets depending on which endpoint is called (`GameRecordService.cs`).
- [ ] `GameRecordEndpoints` PUT removes and re-adds all `GameRecordTags` on every update (even unchanged) and never validates `FormationId`/tag FKs before saving, so a bad id causes an unhandled DB FK exception (500) instead of a 400.
- [ ] `CreateUserSquadDto` flow has a check-then-insert race: two rapid submissions for the same user/game record can both pass the "existing squad" check before either commits; the DB unique index eventually blocks the second insert but as an unhandled `DbUpdateException` (500) rather than a clean 409/400 (`UserSquadEndpoints.cs`).
- [ ] Leaderboard computation does an O(n·m) in-memory scan per squad/player (`UserSquadEndpoints.cs:137-155`) and duplicates statistic-matching logic that already exists in `GameRecordService`.
- [ ] Commented-out DTO fields (`Minutes`, `Number`, `IsCaptain`, `IsSubstitute`) left in `GameRecordService.cs:132-137` — either wire them up or remove the dead code.

### squad-func / squad-domain
- [ ] `SingleMatchHistoricalSearch` isn't idempotent on retry: if the function crashes between uploading to `ai-team-single` and rewriting the source blob, a retry reprocesses the same match and produces duplicates.
- [ ] Score parsing (`GenerateFixtureFromAIMatchData.cs:138-143`, `SeasonDataProcessor.cs:61-62`) assumes a clean `H-A` string; AI output like `"2-2 (AET)"` silently defaults to 0-0 with no logging.
- [ ] Player name matching is exact-string only (`GenerateFixtureFromAIMatchData.cs`), so accents/middle names/suffixes create duplicate `Player` rows instead of reconciling with existing ones.
- [ ] `Player.Name` built as `Firstname + " " + Lastname ?? "N/A"` — the `??` only binds to `Lastname`, so a null first name yields a leading space and a null last name yields `"Firstname N/A"` (`GenerateFixtureFromAIMatchData.cs:283`).
- [ ] Unhandled exceptions mid-processing in `GenerateFixtureFromAIMatchData` re-throw without moving the bad blob to an error container, so the same blob fails on every subsequent timer run — a reprocessing loop that contradicts the AGENTS.md resilience rule.
- [ ] Inconsistent sync/async `SaveChanges()` calls within the same isolated-worker function (`GenerateFixtureFromAIMatchData.cs`) risk blocking the worker thread pool under load.
- [ ] `squad-func/AGENTS.md` references `TeamRefresh.cs`, which does not exist in the current tree — docs are stale relative to code (or the function was removed/renamed and should be reconciled).

## ✨ Features / UX Gaps

- [ ] No captain/vice-captain selection UI — payload always hardcodes `IsCaptain: false, IsViceCaptain: false` (`squad-draft/App.tsx:365-366`); the concept exists in the data model but isn't exposed anywhere in the UI.
- [ ] No way to swap or remove an already-placed player short of a full draft reset — significant UX gap for a drafting game.
- [ ] No pagination on the leaderboard (`squad-draft/components/Leaderboard.tsx`, `squad-api/Endpoints/UserSquadEndpoints.cs`) or on any list endpoint in `squad-api` (players, leagues, fixtures, formations, statistics) — fine today, won't scale.
- [ ] Only recovery path from an API error is a full `window.location.reload()` (`squad-draft/App.tsx:507-512`) — no retry/backoff or partial-failure UI.
- [ ] Email report failures are swallowed with only a `Console.WriteLine` (`squad-func/Services/EmailSMTPService.cs:40-43`) — no alerting if the daily report silently fails to send.

## ⚙️ Improvements

- [ ] Add real retry/backoff for external calls: Gemini AI and sports-data API calls in `squad-func/Services/GeminiService.cs` / `ApiService.cs` are single-shot with no retry on 429/5xx; replace blocking `Thread.Sleep` throttles with `await Task.Delay` plus proper backoff.
- [ ] `FullSeasonAISearch` has no cap on retries for a permanently-failing team/season — since it always picks the oldest un-requested row first, one bad row can block the whole pipeline indefinitely.
- [x] Add `.AsNoTracking()` to all read-only EF Core queries across `squad-api` — currently none use it, despite the project's own `AGENTS.md` requiring it. Fixed: added to 20 read-only queries across the endpoints and `GameRecordService`. By-id `GET`s now use `AsNoTracking().FirstOrDefaultAsync` instead of `FindAsync`; update and delete lookups stay tracked.
- [ ] Review cascade-delete behavior in `squad-api/Models/SquadContext.cs` — no explicit `OnDelete` configured, so deleting a `Fixture`/`Team` could cascade-delete unrelated `GameRecordTag`/`GameRecord` rows via EF Core's default convention.
- [ ] `squad-func/Services/StorageService.cs` calls `CreateIfNotExistsAsync()` on every blob operation — extra round-trip per call, and silently creates a fresh empty container on a typo'd name instead of erroring.
- [ ] `squad-draft/App.tsx` (~35KB) has significant duplicated formation-layout logic vs. `constants.tsx` (~150 overlapping lines) — extract a single shared `computePositionLayout(pos, count, index)` utility.
- [ ] Memoize handlers passed into `Pitch`/`PlayerCard` (`useCallback`) and wrap those components in `React.memo` — currently recreated every render with no memoized children to benefit.
- [ ] Replace `api.ts`'s extensive `any` usage (`team: any`, `p: any`, `payload: any`, `Promise<any | null>`) with typed interfaces matching the C# DTOs — the project's own `AGENTS.md` explicitly bans `any`.
- [ ] Remove leftover `console.log('API_BASE_URL', ...)` debug logging shipped in `squad-draft/api.ts:10`.
- [ ] Consolidate the "strip ```json fences" Gemini-response parsing logic, currently duplicated verbatim in `SingleMatchHistoricalSearch.cs` and `SeasonDataProcessor.cs`, into one shared utility.
- [ ] Replace scattered magic strings for blob container names (`"ai-team"`, `"ai-team-single"`, `"history-completed"`, `"archive"`, etc.) with a shared constants class to avoid typo-created containers.

## 🧹 Tech Debt

- [ ] No automated tests anywhere in the repo (`squad-draft`, `squad-api`, `squad-func`, `squad-domain`) — highest-risk gaps are the pitch/formation layout math (frontend), `GameRecordService`'s statistic-mapping logic (API), and the Gemini JSON-parsing/score-parsing utilities (Functions), all of which are pure logic that's cheap to unit test.
- [ ] Heavy CRUD boilerplate duplication across `squad-api/Endpoints/*.cs` (League, Player, Formation, Fixture, PlayerFixtureStatistic) — nearly identical GET/PUT/POST/DELETE directly against `SquadContext` with no service layer, unlike `GameRecordService`. A generic CRUD helper or shared service pattern would remove ~400 lines of copy-paste.
- [ ] `squad-func/Models/SquadContext.cs` duplicates entity/DbSet definitions that should come from the shared `squad-domain` library — risk of drift between the two contexts over time; worth confirming it isn't accidentally resolving to parallel local type definitions.
- [ ] Inconsistent API response shapes — some `squad-api` endpoints return raw EF entities (shape varies with `ReferenceHandler.IgnoreCycles`), others return hand-built anonymous objects in camelCase alongside PascalCase DTOs elsewhere.
- [ ] Magic-string position codes (`"GK"`, `"@P5"`, etc.) in `GameRecordService.MapPosition` instead of a shared enum from `squad-domain` — risks drift between ingestion (`squad-func`) and display (`squad-api`/frontend).
- [x] Dead code: `squad-func/Models/AgentFixture.cs` and `Models/AI/AiFixture.cs` appear unreferenced; ~150 lines of commented-out classes in `squad-func/Models/PlayerStatsResponse.cs:57-212` should be deleted or restored. Fixed: deleted both files and all the commented-out code in `PlayerStatsResponse.cs`, plus the `using squad_func.Models.AI;` in `GeminiService.cs` that depended on `AiFixture`.
- [x] `getBrowserId` (`squad-draft/App.tsx:335-342`) falls back to `Math.random().toString(36)` when `crypto.randomUUID` is unavailable — weak collision resistance for an anti-abuse identifier. Fixed: the fallback now builds an RFC 4122 v4 UUID from `crypto.getRandomValues` (via `generateUuid`); `Math.random` is no longer used.
