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
- `recordRequest` (`api.ts`) no longer calls the third-party `api.ipify.org` service or sends `ipAddress`/`dateTime`. The `RecordRequest` function now derives both server-side, so only the `device` hint is sent.
- Enhanced contrast across text, badges, and focus rings.
- `fetchFixture` (`api.ts`) now calls the new `GetFixture` Azure Function (`VITE_FUNCTIONS_BASE_URL`) instead of `squad-api`, avoiding the API's cold start. The response shape is unchanged.

### Fixed
- Loading the daily formation no longer silently moves already-placed players to a different line. Spot IDs are a running counter across GK/DEF/MID/FWD, so matching saved players by ID put them in the wrong line whenever the formation shape changed (e.g. 4-4-2 to 3-5-2). The new `remapFormationPlayers` helper in `constants.tsx` matches by line and slot number instead. If a placed player's slot doesn't exist in the new shape, the saved layout and its `formationId` are kept, so the submission stays self-consistent.
- `getBrowserId` no longer falls back to `Math.random()` (about 60 bits, not cryptographically random) when `crypto.randomUUID` is unavailable, e.g. over plain HTTP or on older Safari. A new `generateUuid` helper builds a proper RFC 4122 v4 UUID from `crypto.getRandomValues` instead. IDs already stored in `localStorage` are kept, so existing players stay linked to their leaderboard entries.
- `CookieConsent` no longer collects consent decoratively: the analytics tracking call (`recordRequest`) in `App.tsx` now checks `checkConsent('analytics')` before firing, and reacts immediately to consent changes via a new `squad-cookie-consent-changed` event instead of only running once on mount.

### Removed
- Removed the "Daily Squad Draft Challenge" subtitle text from the header.
- Removed the "Stats" tab and `fetchStatistics` call from `AboutDialog`, which surfaced platform-wide league/team/player/fixture/game counts.

