---
name: reactjs-specialist
description: >-
  Architects, develops, and troubleshoots the React 19 frontend in squad-draft. Specialized in TypeScript, Tailwind CSS, tactical pitch visualization, player drafting state, responsive mobile/desktop UI, and client-side performance.
---

# React.js Specialist Workflow

Use this skill when developing, refactoring, or troubleshooting frontend features in `squad-draft`.

## Core Workflows

### 1. Modifying Pitch & Tactical Formations
- Pitch coordinates and spots live in [`components/Pitch.tsx`](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-draft/components/Pitch.tsx).
- Spots are computed in [`constants.tsx`](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-draft/constants.tsx) via `generateFormationSpots(def, mid, att)`.
- When updating positioning logic:
  - Use percentage-based strings (`top: 'XX%'`, `left: 'YY%'`) to support responsive pitch SVG scaling.
  - Test both drag-and-drop interactions (`onDragStart`, `onDrop`) and touch/click interactions (`onPlayerClick`, `onSpotClick`).

### 2. State & Persistence Updates
- The draft state is tracked in [`App.tsx`](file:///Users/seanrafferty/Documents/development/repos/SquadComplete/squad-draft/App.tsx).
- When altering draft data structures:
  - Update `types.ts` (`DraftState`, `FormationSpot`, `Player`).
  - Maintain backwards compatibility for `localStorage` items (`squad-draft-${today}`).
  - Guard state transitions so `completed` is only true when exactly 11 valid players are chosen.

### 3. Responsive Layout Adjustments
- Verify both viewport layouts:
  - **Desktop (`lg:`)**: Side-by-side split view.
  - **Mobile (`< lg`)**: Stacked view with fixed bottom action bar (`<footer className="md:hidden fixed bottom-0...">`).
  - Include spacer divs (`h-20`) to prevent sticky bottom bars from covering UI controls.

### 4. Build & Validation
Always run the build check after modifications:
```bash
cd squad-draft && npm run build
```

Ensure `squad-draft/CHANGELOG.md` is updated before concluding.
