# Changelog - squad-api

All notable changes to the `squad-api` backend service will be documented in this file.

## [Unreleased]

### Added
- Added `squad-api/AGENTS.md` containing scoped agent guidelines for .NET 8 Minimal APIs, EF Core, and PostgreSQL data persistence.
- Added per-IP fixed-window rate limiting (10 requests/minute) on the public, unauthenticated write endpoints (`POST /api/feedback`, `POST /api/events`, `POST /api/user-squads`) to prevent spam, returning `429 Too Many Requests` when exceeded.

### Changed
- Closed a mass-assignment gap: `PlayerEndpoints`, `LeagueEndpoints`, `FixtureEndpoints`, `FormationEndpoints`, and `PlayerFixtureStatisticEndpoints` now bind `POST`/`PUT` requests to new DTOs (`PlayerDto`, `LeagueDto`, `FixtureDto`, `FormationDto`, `PlayerFixtureStatisticDto`) instead of the raw EF entities, so callers can no longer set `Id`, `CreatedAt`, or `UpdatedAt` directly.
- Removed the Supabase host/username from the committed `appsettings.json` `ConnectionStrings:DefaultConnection` (now an empty placeholder) since production already supplies the real value via an Azure App Service `ConnectionStrings__DefaultConnection` app setting; nothing about the real DB endpoint ships in the repo now.
- Locked down CORS from `AllowAnyOrigin()` to a configured allowlist (`Cors:AllowedOrigins` in `appsettings.json`), set to the production frontend origins (`https://squadup.raffrock.com` custom domain and `https://blue-wave-059115703.4.azurestaticapps.net` default Static Web App hostname) with `http://localhost:3000` overriding it in `appsettings.Development.json` for local dev.
