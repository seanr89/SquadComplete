# Changelog - squad-api

All notable changes to the `squad-api` backend service will be documented in this file.

## [Unreleased]

### Added
- Added `squad-api/AGENTS.md` containing scoped agent guidelines for .NET 8 Minimal APIs, EF Core, and PostgreSQL data persistence.
- Added per-IP fixed-window rate limiting (10 requests/minute) on the public, unauthenticated write endpoints (`POST /api/feedback`, `POST /api/events`, `POST /api/user-squads`) to prevent spam, returning `429 Too Many Requests` when exceeded.
- Added admin API key authentication (`Auth/AdminApiKeyFilter.cs`, applied via `.RequireAdminApiKey()`) to every data-management write endpoint (`POST`/`PUT`/`DELETE` on `/api/players`, `/api/leagues`, `/api/fixtures`, `/api/formations`, `/api/game-records`, `/api/player-fixture-statistics`) and to `GET /api/events` (internal logs). Callers must send the key in the `X-Api-Key` header; it is compared in constant time against `Auth:AdminApiKey` (supplied via the `Auth__AdminApiKey` app setting / user-secrets, empty placeholder in `appsettings.json`). The filter fails closed with `401` if no key is configured. Public gameplay endpoints (`GET` reads, `POST /api/user-squads`, `/api/feedback`, `/api/events`) are unchanged.

### Changed
- Replaced the copy-pasted GET/PUT/POST/DELETE handlers in `LeagueEndpoints`, `PlayerEndpoints`, `FormationEndpoints` and `FixtureEndpoints` with a shared generic `MapCrud<TEntity, TDto>` helper (`Endpoints/CrudEndpointExtensions.cs`). Each endpoint file now only supplies its DbSet-specific mapping (create/update field copies and optional `.Include()`s). Routes, operation IDs, tags, status codes, response bodies and admin API key protection are unchanged (the generated OpenAPI document is identical). `PlayerFixtureStatisticEndpoints` is unchanged because of its composite key.
- Added `.AsNoTracking()` to all read-only EF Core queries, as `AGENTS.md` requires: the `GET` list and by-id handlers for players, leagues, formations, fixtures and player-fixture-statistics, `GET /api/events`, the leaderboard queries and duplicate-squad check in `UserSquadEndpoints`, and all six reads in `GameRecordService`. The by-id handlers switched from `FindAsync` to `AsNoTracking().FirstOrDefaultAsync(...)`. Queries whose results are then updated or deleted (`PUT`/`DELETE` lookups, the user lookup in `POST /api/user-squads`) stay tracked. Responses built from raw entities no longer include navigation properties that change tracking used to fill in.
- Closed a mass-assignment gap: `PlayerEndpoints`, `LeagueEndpoints`, `FixtureEndpoints`, `FormationEndpoints`, and `PlayerFixtureStatisticEndpoints` now bind `POST`/`PUT` requests to new DTOs (`PlayerDto`, `LeagueDto`, `FixtureDto`, `FormationDto`, `PlayerFixtureStatisticDto`) instead of the raw EF entities, so callers can no longer set `Id`, `CreatedAt`, or `UpdatedAt` directly.
- Removed the Supabase host/username from the committed `appsettings.json` `ConnectionStrings:DefaultConnection` (now an empty placeholder) since production already supplies the real value via an Azure App Service `ConnectionStrings__DefaultConnection` app setting; nothing about the real DB endpoint ships in the repo now.
- Locked down CORS from `AllowAnyOrigin()` to a configured allowlist (`Cors:AllowedOrigins` in `appsettings.json`), set to the production frontend origins (`https://squadup.raffrock.com` custom domain and `https://blue-wave-059115703.4.azurestaticapps.net` default Static Web App hostname) with `http://localhost:3000` overriding it in `appsettings.Development.json` for local dev.

### Fixed
- `GET /api/game-records` and `GET /api/game-records/{id}` now exclude substitutes (`IsSubstitute == false`), matching `GET /api/game-records/date/{date}`, so a game record returns the same player set from every endpoint.
- `PUT /api/game-records/{id}` now validates `FormationId` and every tag's fixture/team id up front and returns `400 Bad Request` instead of an unhandled FK exception (500). Tags are now diffed by fixture/team pair so unchanged tags are left alone rather than deleted and re-inserted, and `FormationId` is now actually persisted on update.

### Removed
- Removed `GET /api/statistics` (`StatisticsEndpoints.cs`, `StatisticsDto.cs`), which returned platform-wide league/team/player/fixture/game counts, along with its registration in `EndpointExtensions.cs`. No longer used now that the "Stats" tab has been removed from the frontend's `AboutDialog`.
