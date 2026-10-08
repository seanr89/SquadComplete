# TimerTrigger - C<span>#</span>

The `TimerTrigger` makes it incredibly easy to have your functions executed on a schedule. This sample demonstrates a simple use case of calling your function every 5 minutes.

## How it works

For a `TimerTrigger` to work, you provide a schedule in the form of a [cron expression](https://en.wikipedia.org/wiki/Cron#CRON_expression)(See the link for full details). A cron expression is a string with 6 separate expressions which represent a given schedule via patterns. The pattern we use to represent every 5 minutes is `0 */5 * * * *`. This, in plain text, means: "When seconds is equal to 0, minutes is divisible by 5, for any hour, day of the month, month, day of the week, or year".

## Learn more

<TODO> Documentation
## Development Guidelines

**Important:** After each successful merge into the main branch, update `CHANGELOG.md` with the new changes and update relevant documentation.

## Current Functions

- **CleanupGameRecords**:
  - **Trigger**: `0 0 2 * * 1` (Weekly on Monday at 02:00 UTC)
  - **Description**: Cleans up database game records older than a configured threshold of days (defaults to 60 days, configurable via `GameRecordsCleanupDays` environment variable).

- **DailyReport**:
  - **Trigger**: `0 0 1 * * *` (Daily at 01:00 UTC)
  - **Description**: Generates and emails a daily status report (containing counts for total/active teams, players, fixtures, game records, user squads, missing info, and AI fixtures) to the administrator.

- **GetFixture**:
  - **Trigger**: HTTP GET, anonymous (Route: `fixtures/{id:int}`)
  - **Description**: Returns a fixture with its league, mirroring `squad-api`'s `GET /api/fixtures/{id}`. Responses are cacheable for an hour. Returns 404 if the fixture doesn't exist.

- **GetGameRecordByDate**:
  - **Trigger**: HTTP GET, anonymous (Route: `game-records/date/{date}`, `date` as `yyyy-MM-dd`)
  - **Description**: Returns the game record for that day (formation, teams and starting-XI players with positions and ratings), mirroring `squad-api`'s `GET /api/game-records/date/{date}`. Returns 400 for a malformed date and 404 if the day has no record.

- **RecordRequest**:
  - **Trigger**: HTTP POST (Route: `record`)
  - **Description**: Receives a JSON request body containing a datetime, incoming IP address, and device. It logs the event details and returns a confirmation response containing the processed values (falling back to headers if IP/Device are not specified in the body).

- **SquadSelector**: 
  - **Trigger**: `0 0 2 * * *` (Daily at 02:00 UTC)
  - **Description**: Automatically creates the daily Game Record. It selects a random formation, shuffles historical fixtures, and selects 11 unique active teams that have at least 11 players with positions and ratings.
