# Changelog - squad-func

All notable changes to the `squad-func` Azure Functions service will be documented in this file.

## [Unreleased]

### Added
- Added `squad-func/AGENTS.md` containing scoped agent guidelines for Azure Functions, Gemini AI search pipelines, and blob ingestion triggers.
- Added `IpRateLimiterService`, an in-memory per-IP fixed-window limiter, and wired it into the anonymous `RecordRequest` HTTP trigger (10 requests/minute, keyed off the server-derived IP, not the client-asserted body value) to prevent spam.
- Added `GetFixture`, an anonymous HTTP `GET /api/fixtures/{id}` trigger that mirrors `squad-api`'s `GetFixtureById` (fixture with its `League` included, same JSON shape) so the client's fixture lookup doesn't wait on the API's cold start. Responses carry `Cache-Control: public, max-age=3600` since historic fixtures don't change.

### Fixed
- `RecordRequest` no longer trusts client-asserted audit fields: the IP address is now derived server-side from the right-most `X-Forwarded-For` entry (the one Azure's front end appends, with any port stripped), falling back to the connection's remote address, and the timestamp is always `DateTime.UtcNow`. `ipAddress`/`dateTime` in the request body are ignored. The device string prefers the `User-Agent` header over the body and is capped at 512 characters. The rate limiter now keys off the same resolved IP rather than the raw, spoofable `X-Forwarded-For` string.
- `GenerateFixtureFromAIMatchData` now validates and cleans Gemini output before writing to the database, using the new `Utils/AiDataSanitizer`:
  - All AI-supplied text (competition, team and player names, positions) is trimmed and whitespace-collapsed, stripped of control and invisible characters (accented and non-Latin letters are kept), and truncated to the `squad-domain` `[StringLength]` limits (255 for names, 50 for positions). This prevents `SaveChanges` failures on over-long AI output.
  - Player ratings are rounded to 1dp to match the `NUMERIC(3,1)` column; missing, non-finite or out-of-range (outside 0–10) ratings are stored as `0`.
  - The final score must be in the prompt's `"X-X"` format with values 0–99. Previously an unparseable score silently became 0-0.
  - Payloads with a missing competition or team, an unparseable date or score, or the same team on both sides are logged and moved to a new `archive-invalid-data` container, so they aren't retried on every run.
  - Players whose names are empty after cleaning are dropped, and duplicate names within a team are ignored.
  - Names coming back from the sports API are cleaned too, falling back to the AI name. This also fixes a `Firstname + " " + Lastname ?? "N/A"` expression whose fallback could never apply.
- `MatchDataUtils.GetMatchDate` now returns `null` for a missing or unparseable date (parsed with the invariant culture). Previously it returned `DateTime.MinValue`, so the caller's null check never fired and bad dates were written as `0001-01-01`.

### Removed
- Removed dead code: the unreferenced `Models/AgentFixture.cs` (`AgentFixture`, `AgentMatch`, `AgentMatchDetails`, `AgentScore`, `AgentLineups`) and `Models/AI/AiFixture.cs`, about 130 lines of commented-out statistic properties and classes in `Models/PlayerStatsResponse.cs`, and the now-empty `using squad_func.Models.AI;` in `Services/GeminiService.cs`.
