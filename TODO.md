# TODO — SquadComplete Review Findings

Generated from a full-codebase review of `squad-draft`, `squad-api`, `squad-func`, and `squad-domain` (2026-09-26). Items are grouped by category; each entry cites the file(s) it applies to. Not exhaustive — prioritized for impact.

## 🐞 Bugs

### squad-draft
- [ ] `isDraftComplete` (`App.tsx:117`) is computed but never used to gate completion — only `draft.completed` (set when `selectedPlayers` reaches exactly 11) does. If the API ever returns fewer than 11 daily squads, users get stuck and can never reach submission.
- [ ] `executeResetDraft` (`App.tsx:290-302`) doesn't preserve/clear `gameRecordId`/`formationId` — after a reset these remain `undefined` for the rest of the session since the fetch effect only runs once (`[]` deps).
- [ ] `submitUserSquad` coerces a missing player id to `0` (`parseInt(spot.player?.id || '0', 10)`, `App.tsx:363`) instead of validating all 11 spots are filled before submit.
- [ ] "Advancing to pick number X of 11" message (`App.tsx:275`) relies on a stale-closure value of `draft.currentStep` rather than the freshly computed next step — works today but is fragile.
- [ ] `localStorage.setItem` failures (e.g. `QuotaExceededError`) are only logged, not surfaced to the user — draft silently stops persisting (`App.tsx:104-111`).

### squad-api
- [ ] `CreateUserSquadDto` flow has a check-then-insert race: two rapid submissions for the same user/game record can both pass the "existing squad" check before either commits; the DB unique index eventually blocks the second insert but as an unhandled `DbUpdateException` (500) rather than a clean 409/400 (`UserSquadEndpoints.cs`).
- [ ] Leaderboard computation does an O(n·m) in-memory scan per squad/player (`UserSquadEndpoints.cs:137-155`) and duplicates statistic-matching logic that already exists in `GameRecordService`.
- [ ] Commented-out DTO fields (`Minutes`, `Number`, `IsCaptain`, `IsSubstitute`) left in `GameRecordService.cs:132-137` — either wire them up or remove the dead code.

## ✨ Features / UX Gaps

- [ ] No captain/vice-captain selection UI — payload always hardcodes `IsCaptain: false, IsViceCaptain: false` (`squad-draft/App.tsx:365-366`); the concept exists in the data model but isn't exposed anywhere in the UI.
- [ ] No way to swap or remove an already-placed player short of a full draft reset — significant UX gap for a drafting game.
- [ ] No pagination on the leaderboard (`squad-draft/components/Leaderboard.tsx`, `squad-api/Endpoints/UserSquadEndpoints.cs`) or on any list endpoint in `squad-api` (players, leagues, fixtures, formations, statistics) — fine today, won't scale.
- [ ] Only recovery path from an API error is a full `window.location.reload()` (`squad-draft/App.tsx:507-512`) — no retry/backoff or partial-failure UI.

## ⚙️ Improvements

- [ ] Add real retry/backoff for external calls: Gemini AI and sports-data API calls in `squad-func/Services/GeminiService.cs` / `ApiService.cs` are single-shot with no retry on 429/5xx; replace blocking `Thread.Sleep` throttles with `await Task.Delay` plus proper backoff.
- [ ] Review cascade-delete behavior in `squad-domain/Models/SquadContext.cs` — no explicit `OnDelete` configured, so deleting a `Fixture`/`Team` could cascade-delete unrelated `GameRecordTag`/`GameRecord` rows via EF Core's default convention.
- [ ] `squad-draft/App.tsx` (~35KB) has significant duplicated formation-layout logic vs. `constants.tsx` (~150 overlapping lines) — extract a single shared `computePositionLayout(pos, count, index)` utility.
- [ ] Memoize handlers passed into `Pitch`/`PlayerCard` (`useCallback`) and wrap those components in `React.memo` — currently recreated every render with no memoized children to benefit.
- [ ] Replace `api.ts`'s extensive `any` usage (`team: any`, `p: any`, `payload: any`, `Promise<any | null>`) with typed interfaces matching the C# DTOs — the project's own `AGENTS.md` explicitly bans `any`.
- [ ] Remove leftover `console.log('API_BASE_URL', ...)` debug logging shipped in `squad-draft/api.ts:10`.

## 🧹 Tech Debt

- [ ] No automated tests anywhere in the repo (`squad-draft`, `squad-api`, `squad-func`, `squad-domain`) — highest-risk gaps are the pitch/formation layout math (frontend), `GameRecordService`'s statistic-mapping logic (API), and the Gemini JSON-parsing/score-parsing utilities (Functions), all of which are pure logic that's cheap to unit test.
- [x] `squad-func/Models/SquadContext.cs` duplicated entity/DbSet definitions that should come from the shared `squad-domain` library — risk of drift between the two contexts over time; worth confirming it isn't accidentally resolving to parallel local type definitions.
- [ ] Inconsistent API response shapes — some `squad-api` endpoints return raw EF entities (shape varies with `ReferenceHandler.IgnoreCycles`), others return hand-built anonymous objects in camelCase alongside PascalCase DTOs elsewhere.
- [x] Magic-string position codes (`"GK"`, `"@P5"`, etc.) in `GameRecordService.MapPosition` instead of a shared enum from `squad-domain` — risks drift between ingestion (`squad-func`) and display (`squad-api`/frontend).
