# AGENTS.md — SquadComplete Repository Guide

Welcome to the **SquadComplete** repository. This document serves as the master guide for all AI agents, pair programmers, and autonomous tools working within this codebase.

---

## 1. Project Overview & Architecture

SquadComplete is a monorepo powering **Ultimate 11: Squad Draft**, a daily football (soccer) drafting game where users assemble fantasy squads from historic and curated football lineups.

### Repository Map

- **[squad-draft](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-draft)**: Client-side single-page application built with **React 19**, **TypeScript**, **Vite**, and **Tailwind CSS**. Deployed to Azure Static Web Apps.
- **[squad-api](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-api)**: Web API backend built with **.NET 8 / C# Minimal APIs**, **Entity Framework Core**, and **PostgreSQL (Npgsql)**. Includes Scalar & OpenAPI documentation.
- **[squad-func](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-func)**: **Azure Functions** service in C# handling scheduled cron tasks, historical match ingestion, Gemini AI team searches, and blob processing.
- **[squad-domain](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-domain)**: Shared C# class library containing core domain models, enums, and database entities.
- **[sql](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/sql)**: Database schema definitions, seed scripts, and migration files.
- **[Makefile](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/Makefile)**: Common build, test, and branch maintenance automation.

---

## 2. Mandatory Workflow Rules

All agents working on this repository **must** strictly adhere to the following rules:

### Rule 1: Changelog Maintenance (Strictly Enforced)
After every feature change, bug fix, or merge into the `main` branch, you **must update the `CHANGELOG.md` file in each affected subproject directory**:
- `squad-draft/CHANGELOG.md`
- `squad-api/CHANGELOG.md`
- `squad-func/CHANGELOG.md`
- Root `CHANGELOG.md` (when changes span multiple services or repo-level tooling)

Keep changelog entries structured under standard Keep a Changelog categories (`Added`, `Changed`, `Fixed`, `Removed`).

### Rule 2: Clean Build Gate
Before finalizing any modification, verify that the affected subproject builds cleanly:
- Frontend: `npm run build` inside `squad-draft/`
- API Backend: `dotnet build squad-api`
- Azure Functions: `dotnet build squad-func`
- Or run `make all` from the repository root.

### Rule 3: Zero Secret Leakage
Never hardcode API keys, database connection strings, or service tokens in any source file or committed environment file. Always use environment variables (`.env.local` for Vite, `appsettings.Development.json` for API, `local.settings.json` for Azure Functions).

---

## 3. Specialist Agent Personas & Delegation

This repository defines specialized personas and workflow procedures under [`.agents/`](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/.agents):

| Persona | Configuration / Skill | Focus Area |
| :--- | :--- | :--- |
| **Code Reviewer** | [`.agents/code-reviewer.md`](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/.agents/code-reviewer.md) <br> [Skill: code-reviewer](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/.agents/skills/code-reviewer/SKILL.md) | Quality gatekeeper, security review, performance audit, changelog verification, and PR review. |
| **React.js Specialist** | [`.agents/reactjs-specialist.md`](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/.agents/reactjs-specialist.md) <br> [Skill: reactjs-specialist](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/.agents/skills/reactjs-specialist/SKILL.md) | React 19 architecture, Vite tooling, pitch drag-and-drop mechanics, Tailwind responsive UI, and state management. |
| **.NET Specialist** | [`.agents/dotnet-specialist.md`](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/.agents/dotnet-specialist.md) | .NET 8 Minimal APIs, EF Core query optimization, Azure Functions triggers, and data pipelines. |

When tackling multi-faceted tasks, delegate or adopt these specialist personas to ensure domain-specific best practices are applied.

---

## 4. Directory-Specific Rules

For detailed subproject instructions, refer to the local `AGENTS.md` files:
- [squad-draft/AGENTS.md](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-draft/AGENTS.md)
- [squad-api/AGENTS.md](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-api/AGENTS.md)
- [squad-func/AGENTS.md](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-func/AGENTS.md)
