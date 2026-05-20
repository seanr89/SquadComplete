# TimerTrigger - C<span>#</span>

The `TimerTrigger` makes it incredibly easy to have your functions executed on a schedule. This sample demonstrates a simple use case of calling your function every 5 minutes.

## How it works

For a `TimerTrigger` to work, you provide a schedule in the form of a [cron expression](https://en.wikipedia.org/wiki/Cron#CRON_expression)(See the link for full details). A cron expression is a string with 6 separate expressions which represent a given schedule via patterns. The pattern we use to represent every 5 minutes is `0 */5 * * * *`. This, in plain text, means: "When seconds is equal to 0, minutes is divisible by 5, for any hour, day of the month, month, day of the week, or year".

## Learn more

<TODO> Documentation
## Development Guidelines

**Important:** After each successful merge into the main branch, update `CHANGELOG.md` with the new changes and update relevant documentation.

## Current Functions

- **FullSeasonAISearch**: 
  - **Trigger**: `0 0 16 * * *` (Daily at 16:00)
  - **Description**: Fetches historical team season data using the Gemini AI service. It picks one unrequested active `TeamSeason`, queries the history, sanitizes the response (removes losses), uploads the JSON to the `squad-history` blob container, and updates the database record.

- **SingleMatchHistoricalSearch**: 
  - **Trigger**: `0 15,45 15-19 * * *` (Daily at minute 15 and 45 between 15:00 and 19:59)
  - **Description**: Iterates through the list of fixtures within the `ai-team` container blobs. For each match, it queries detailed Gemini AI analytics for the single match, deserializes the clean response, saves it into the `ai-team-single` container, and handles the iteration logic by updating/removing source fixtures.

- **SquadSelector**: 
  - **Trigger**: `0 0 2 * * *` (Daily at 02:00)
  - **Description**: Responsible for automated creation of daily Game Records. It randomizes a formation, shuffles historical fixtures, and attempts to assemble 11 unique active teams that contain sufficient historical player data to form a complete squad assignment for the day's trivia or activity.

- **TeamRefresh**: 
  - **Trigger**: `0 0 5-7 * * *` (Daily at 05:00, 06:00, and 07:00)
  - **Description**: Discovers fixtures in the database that lack specific date and lineup information. It communicates with the External Sports API to obtain accurate final scores, team IDs, and precise kickoff timestamps, limiting the rate to 4 updates per run with deliberate rate-limiting delay.
