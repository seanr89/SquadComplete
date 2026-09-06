# AGENTS.md — squad-draft (Frontend Service)

This file contains scoped instructions and guidelines for AI agents working within the `squad-draft` directory.

---

## 1. Technical Stack & Environment

- **Framework**: React 19 (`react` ^19.2.3, `react-dom` ^19.2.3)
- **Language**: TypeScript (`~5.8.2`) with strict mode enabled
- **Build Tool**: Vite (`^6.2.0`)
- **Styling**: Tailwind CSS (with Font Awesome icons via CDN)
- **Utilities**: `html2canvas` for client-side pitch rendering / image export
- **Hosting Target**: Azure Static Web Apps (configured via `staticwebapp.config.json`)

---

## 2. Architecture & State Management

The core application flow represents a daily draft challenge consisting of:
1. **Challenge Ingestion**: Fetches daily challenge and formation from `fetchDailySquads()` in `api.ts`.
2. **Draft State**:
   - Persisted daily in `localStorage` under key `squad-draft-${YYYY-MM-DD}`.
   - Tracks `currentStep` (0 to 11), `selectedPlayers`, `formation`, `completed`, and `submitted`.
   - Temporary placement state (`tempPlayer`, `activeSpotId`) handles the two-step selection flow (Click player -> Click formation spot).
3. **Pitch Coordinates**:
   - `Pitch.tsx` displays formation spots with relative percentage-based styling (`top`, `left`).
   - Supports both click-to-place and HTML5 Drag & Drop (`draggable`, `onDragStart`, `onDrop`).
4. **Submission**:
   - Submits team payload with `BrowserIdentifierId` (`squad-browser-id`), `UserName`, `GameRecordId`, `FormationId`, and `Players`.
   - Deduplicated daily by backend using the browser ID / game record.

---

## 3. Frontend Conventions & Best Practices

- **React 19 Hooks**:
  - Prefer modern hooks (`useState`, `useMemo`, `useCallback`, `useRef`).
  - Avoid redundant state that can be derived (e.g. `totalRating` and `isDraftComplete` are derived with `useMemo`).
- **No Direct Mutation**: Always treat state immutably, especially nested arrays like `draft.formation`.
- **Responsive Design**:
  - Desktop view utilizes side-by-side grids (Selection / Pitch or Pitch / Summary).
  - Mobile view uses stacked layouts and a fixed bottom navigation bar (`md:hidden fixed bottom-0`).
- **Dialogs & Modals**:
  - Utilize accessible overlay wrappers (`AlertDialog`, `AboutDialog`, `CookieConsent`).
  - Ensure modals handle backdrop clicks and escape keys gracefully.
- **Strict Typing**:
  - All domain interfaces live in `types.ts` (`Player`, `Squad`, `FormationSpot`, `DraftState`, `Position`).
  - Do not use `any`; use strict Discriminated Unions and type guards where appropriate.

---

## 4. Verification Commands

Run from the `squad-draft/` directory:
```bash
# Type check and build bundle
npm run build

# Start local development server
npm run dev

# Preview production build
npm run preview
```

---

## 5. Changelog Requirement

Every change in `squad-draft` **must** be documented in `squad-draft/CHANGELOG.md` under the appropriate section before closing out work.
