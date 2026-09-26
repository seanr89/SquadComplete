# Changelog - squad-draft

All notable changes to the `squad-draft` frontend service will be documented in this file.

## [Unreleased]

### Added
- Created `TeamRoster` component displaying drafted squad members categorized by tactical lines (Goalkeeper, Defenders, Midfielders, Forwards) with interactive hover synchronization to the pitch.
- Added highlighted state and hover synchronization between `TeamRoster` and `Pitch`.
- Added `squad-draft/AGENTS.md` containing scoped agent guidelines for React 19, Vite, Tailwind CSS, and pitch drafting mechanics.
- Configured React.js Specialist agent persona and workflow instructions.
- Full keyboard navigation support across player drafting (`Tab`, `Shift+Tab`, `Enter`, `Space`, `Escape`).
- Dedicated screen reader live region (`aria-live="polite"`) announcing step progress, player selection, and draft completion.
- "Skip to main content" link for keyboard and screen reader accessibility.
- Fallback avatar rendering with player initials and position for missing or failed images.
- In-app accessible modal confirmation for draft reset replacing `window.confirm`.
- Support for `prefers-reduced-motion` media queries.

### Changed
- Expanded midfield formation spacing across the tactical pitch with wider horizontal distribution and natural vertical depth staggering (e.g. LCM/RCM flanking with a deeper central pivot) to eliminate avatar clumping.
- Redesigned Team and Roster screen with a modern Hero Command Center header displaying glowing average rating, squad progress, inline submission, WhatsApp sharing, and reset controls.
- Replaced cramped 1/3 sidebar roster list with a balanced split view (`Pitch` and spacious `TeamRoster`), eliminating player name truncation.
- Modernized player cards in roster with circular position-accented avatars, high-contrast typography, and gold gradient rating pills (removing harsh white square backgrounds in dark mode).
- Refactored `PlayerCard` into fully accessible interactive elements with `role="button"`, ARIA labels, focus rings, and keyboard event handlers.
- Replaced hover-only instructions with an accessible toggle button and popover.
- Enhanced contrast across text, badges, and focus rings.

### Fixed
- `CookieConsent` no longer collects consent decoratively: the analytics tracking call (`recordRequest`) in `App.tsx` now checks `checkConsent('analytics')` before firing, and reacts immediately to consent changes via a new `squad-cookie-consent-changed` event instead of only running once on mount.

### Removed
- Removed the "Daily Squad Draft Challenge" subtitle text from the header.

