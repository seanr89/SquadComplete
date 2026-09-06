# Changelog - squad-draft

All notable changes to the `squad-draft` frontend service will be documented in this file.

## [Unreleased]

### Added
- Added `squad-draft/AGENTS.md` containing scoped agent guidelines for React 19, Vite, Tailwind CSS, and pitch drafting mechanics.
- Configured React.js Specialist agent persona and workflow instructions.
- Full keyboard navigation support across player drafting (`Tab`, `Shift+Tab`, `Enter`, `Space`, `Escape`).
- Dedicated screen reader live region (`aria-live="polite"`) announcing step progress, player selection, and draft completion.
- "Skip to main content" link for keyboard and screen reader accessibility.
- Fallback avatar rendering with player initials and position for missing or failed images.
- In-app accessible modal confirmation for draft reset replacing `window.confirm`.
- Support for `prefers-reduced-motion` media queries.

### Changed
- Refactored `PlayerCard` into fully accessible interactive elements with `role="button"`, ARIA labels, focus rings, and keyboard event handlers.
- Refactored `Pitch` with semantic `role="region"` and accessible empty spot buttons.
- Refactored `AlertDialog`, `AboutDialog`, `CookieConsent`, `MaterialDatePicker`, and `Leaderboard` squad details to comply with WAI-ARIA modal dialog patterns with `Escape` key dismissal and focus management.
- Replaced hover-only instructions with an accessible toggle button and popover.
- Enhanced contrast across text, badges, and focus rings.

