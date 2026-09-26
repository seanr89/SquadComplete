# Changelog - squad-func

All notable changes to the `squad-func` Azure Functions service will be documented in this file.

## [Unreleased]

### Added
- Added `squad-func/AGENTS.md` containing scoped agent guidelines for Azure Functions, Gemini AI search pipelines, and blob ingestion triggers.
- Added `IpRateLimiterService`, an in-memory per-IP fixed-window limiter, and wired it into the anonymous `RecordRequest` HTTP trigger (10 requests/minute, keyed off the server-derived IP, not the client-asserted body value) to prevent spam.
