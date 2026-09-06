# AGENTS.md — squad-api (Backend Service)

This file contains scoped instructions and guidelines for AI agents working within the `squad-api` directory.

---

## 1. Technical Stack & Environment

- **Framework**: .NET 8 (ASP.NET Core Minimal APIs)
- **Language**: C# 12
- **ORM**: Entity Framework Core 8 with PostgreSQL provider (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **API Documentation**: Microsoft OpenAPI + Scalar (`Scalar.AspNetCore`)
- **Architecture Pattern**: Minimal APIs with modular endpoint mapping extensions

---

## 2. Architecture & Directory Structure

- **`Program.cs`**: Application entry point, dependency injection container, JSON serialization settings (handles circular references via `ReferenceHandler.IgnoreCycles`), CORS policies, OpenAPI, and pipeline registration.
- **`Endpoints/`**: Endpoint route groupings (e.g., GameRecord, Squad, Leaderboard). All registered via `MapAllEndpoints()` in `EndpointExtensions.cs`.
- **`Services/`**: Business logic layer (e.g., `GameRecordService.cs`). Injected as scoped services into Minimal API route handlers.
- **`Models/` & `squad-domain/`**: Entity Framework entities, database context (`SquadContext`), and domain models.
- **`DTOs/`**: Request and response contracts decoupling database models from public HTTP APIs.

---

## 3. Backend Conventions & Best Practices

- **Endpoint Handlers**:
  - Keep endpoint handlers lean: parse input, delegate to services, and return typed results (`Results.Ok()`, `Results.NotFound()`, `Results.BadRequest()`).
  - Use `[FromServices]` or parameter injection for dependencies.
- **Entity Framework & Database**:
  - Always use asynchronous EF Core queries (`ToListAsync()`, `FirstOrDefaultAsync()`).
  - Use `.AsNoTracking()` for read-only operations to maximize performance.
  - Guard against N+1 query problems by using explicit `.Include()` / `.ThenInclude()` navigation loading where necessary.
- **CORS & Security**:
  - Ensure CORS headers allow preflight and requests from frontend clients.
  - Never commit raw connection strings or production credentials; utilize `appsettings.Development.json` or environment variables (`ConnectionStrings__DefaultConnection`).

---

## 4. Verification Commands

Run from the repository root or within `squad-api/`:
```bash
# Build the API project
dotnet build squad-api

# Run local API instance
dotnet run --project squad-api

# Execute HTTP test requests
# See squad-api/squad-api.http for test request collections
```

---

## 5. Changelog Requirement

Every change in `squad-api` **must** be documented in `squad-api/CHANGELOG.md` under the appropriate section before closing out work.
