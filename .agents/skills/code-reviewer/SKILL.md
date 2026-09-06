---
name: code-reviewer
description: >-
  Audits pull requests, git diffs, and codebase modifications across the SquadComplete repository. Enforces coding standards, security, React 19 / .NET best practices, build verification, and mandatory CHANGELOG updates.
---

# Code Reviewer Workflow

Use this skill when tasked with reviewing code changes, auditing pull requests, or verifying the safety and quality of modifications across `squad-draft`, `squad-api`, and `squad-func`.

## Review Procedure

### Step 1: Inspect Changes & Diffs
1. Review the git diff or modified files.
2. Identify all affected subsystems (frontend, API, Azure Functions, or shared domain).

### Step 2: Quality & Security Checklist
Verify the changes against the quality checklist in [`.agents/code-reviewer.md`](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/.agents/code-reviewer.md):
- **React 19 Frontend**: Strict TypeScript types, proper hook usage, no direct state mutations, responsive styling, safe local storage parsing.
- **.NET API**: Asynchronous I/O, `.AsNoTracking()` for read queries, proper HTTP status codes, structured logging.
- **Azure Functions**: Idempotent execution, rate limiting on external APIs (Gemini AI, sports providers), clean blob archiving.
- **Security**: No secrets or connection strings committed in plaintext.

### Step 3: Verify Subproject Builds
Ensure all touched services compile cleanly:
```bash
# Frontend build check
cd squad-draft && npm run build

# Backend API build check
dotnet build squad-api

# Functions build check
dotnet build squad-func
```

### Step 4: Validate Changelog Entries
Confirm that each affected subproject has an updated `CHANGELOG.md` entry documenting the change:
- `squad-draft/CHANGELOG.md`
- `squad-api/CHANGELOG.md`
- `squad-func/CHANGELOG.md`

### Step 5: Deliver Review Output
Format review feedback with clear severity classifications:
- **Blockers**: Critical bugs, security vulnerabilities, broken builds, or missing changelog updates.
- **Warnings**: Potential performance regressions, edge-case bugs, or anti-patterns.
- **Suggestions**: Refactoring recommendations and non-blocking style improvements.
- **Praise**: Notable positive patterns.
