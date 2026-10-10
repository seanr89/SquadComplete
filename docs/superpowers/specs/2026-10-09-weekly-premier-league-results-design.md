# Weekly Premier League Results Search — Design

**Date:** 2026-10-09
**Project:** `squad-func`
**Status:** Draft, awaiting review

## Goal

Each Tuesday, a new Azure Function asks Gemini (grounded with live Google Search) for every
English Premier League match played in the previous seven days, and emails the results to the
administrator as an HTML table.

This is a read-only first step. It lets us judge how reliable Gemini's grounded match data is
before we rebuild database ingestion, which was removed in `264b59d`.

## Scope

**In scope**
- New timer-triggered function `WeeklyMatchSearch`.
- New grounded Gemini call and prompt for a date-range results search.
- Parsing the response into a typed model, rendering an HTML email, logging the results.

**Out of scope**
- Database writes (no `Fixture`, `Team`, `Player` or `PlayerFixtureStatistic` rows).
- Blob storage. The `playersname`-only container rule in `CLAUDE.md` and `squad-func/AGENTS.md` stays as it is.
- Lineups, goalscorers or other per-player data.
- Leagues other than the Premier League. The league name is a constant, not a setting.
- Changes to the existing `GenerateContentAsync` / `agent-prompt.md` (currently unused).

## Decisions

| Question | Decision |
|---|---|
| What happens to the results | Email plus logs only |
| Search window | The previous 7 days, Tuesday to Monday inclusive, in a single Gemini search |
| Per-match content | Date, home team, away team, final score, status |
| Approach | Dedicated prompt + grounded Gemini method + typed parsing, falling back to raw text |

## Components

### `WeeklyMatchSearch.cs` (new, project root)

- Timer trigger `0 0 8 * * 2`: Tuesdays at 08:00 UTC.
- Constructor-injected `GeminiService`, `EmailSMTPService`, `ILogger<WeeklyMatchSearch>`. No `SquadContext`.
- `private const string League = "English Premier League";`
- Works out the window from `DateTime.UtcNow.Date`: `to = today - 1 day` (Monday), `from = today - 7 days` (the previous Tuesday).
  The calculation is relative to the run date, so a manual run on another day still covers the 7 days before it.
- Calls `GetWeeklyResultsAsync`, builds the email (see Data flow), sends it, and logs a one-line summary per match.

### `Services/GeminiService.cs` (modified)

- New `public async Task<WeeklyResultsResponse?> GetWeeklyResultsAsync(string league, DateOnly from, DateOnly to)`.
  - Loads `prompts/weekly-results-prompt.md` and fills in `{LEAGUE}`, `{FROM_DATE}` and `{TO_DATE}` (`yyyy-MM-dd`).
  - Sends the request with Google Search grounding enabled.
  - Returns `null` on an HTTP failure, a timeout or an exception (logged), in the same style as `GetPlayerPhotoPrompt`.
  - Otherwise it returns `WeeklyResultsParser.FromGeminiResponse(responseJson)`, a `WeeklyResultsResponse` containing:
    - `RawText`: the model's text, taken by joining every `candidates[0].content.parts[].text`.
    - `Results`: a `WeeklyResults?` parsed from `RawText`, or `null` if parsing failed.
    - `Sources`: a list of `(Title, Uri)` from `candidates[0].groundingMetadata.groundingChunks[].web`, or empty.
- `BuildBaseRequestBody(string userPrompt, bool useGoogleSearch = false)`: when `true`, adds
  `tools = new[] { new { google_search = new { } } }`. Existing callers keep the default and don't change.
- Parsing lives in a pure static `Services/WeeklyResultsParser.cs` (no I/O, never throws). It takes the text from the first `{` to the last `}`,
  which ignores any code fences or prose around the JSON, and deserializes with case-insensitive, snake_case options that also accept numbers sent as strings. `responseMimeType` is **not** set,
  because Gemini doesn't reliably support JSON mode together with the search tool.
- Uses the existing `_agentModel` (`gemini-3.1-flash-lite`). A 120-second `CancellationTokenSource` matches `GetPlayerPhotoPrompt`.

### `prompts/weekly-results-prompt.md` (new)

- Placeholders: `{LEAGUE}`, `{FROM_DATE}`, `{TO_DATE}`.
- Tells the model to use web search, include only matches **played** between the two dates (inclusive),
  never invent results, and reply with **only** this JSON:

```json
{
  "league": "English Premier League",
  "from_date": "2026-09-29",
  "to_date": "2026-10-05",
  "matches": [
    {
      "date": "2026-10-04",
      "home_team": "Arsenal",
      "away_team": "Chelsea",
      "home_score": 2,
      "away_score": 1,
      "status": "FT"
    }
  ]
}
```

- `status` is `FT`, `AET`, `PEN`, `P-P` (postponed) or `ABD` (abandoned). Scores are `null` when the match wasn't completed.
- A week with no matches returns `"matches": []`.
- Added to `squad-func.csproj` with `CopyToOutputDirectory`, like `agent-prompt.md`.

### `Models/WeeklyResults.cs` (new)

- `WeeklyResults { League, FromDate, ToDate, List<WeeklyMatch> Matches }`
- `WeeklyMatch { Date, HomeTeam, AwayTeam, int? HomeScore, int? AwayScore, Status }`
- `WeeklyResultsResponse { string RawText, WeeklyResults? Results, List<GroundingSource> Sources }`
- `GroundingSource { Title, Uri }`

### `Models/WeeklyResultsEmail.cs` (new)

- `static (string Subject, string Body) WeeklyResultsEmail.Build(DateOnly from, DateOnly to, WeeklyResultsResponse? response)`, where a `null` response means the Gemini call failed.
- Reuses the visual style of `DailyStats.ToHtml()` (same font stack, card and table styling). All Gemini-supplied strings are HTML-encoded with `WebUtility.HtmlEncode`.
  Source links are only rendered as `href`s for `http(s)` URIs.
- Matches dated outside `from`–`to` are left out, and a "Left out N match(es) dated outside this window." note is added.
  Dates with a time part are judged by their first 10 characters. Matches with an unreadable date are kept.

## Data flow

1. The timer fires on Tuesday at 08:00 UTC, and `from`/`to` are calculated.
2. `GetWeeklyResultsAsync("English Premier League", from, to)` runs.
3. The email is built according to the outcome:

| Outcome | Subject | Body |
|---|---|---|
| Parsed, ≥1 match | `Premier League Results — {from:dd MMM} to {to:dd MMM yyyy}` | Table sorted by date, then home team: Date · Home · Score · Away · Status. Then a source list. |
| Parsed, 0 matches | same | "No Premier League matches found for this window." plus sources |
| Parse failed | `… (unparsed)` | Warning banner, then Gemini's raw text in a `<pre>` (HTML-encoded), then sources |
| Gemini call failed (`null`) | `… (FAILED)` | "Gemini search failed. Check function logs." |

4. `EmailSMTPService.SendEmail(recipient: "srafferty89@gmail.com", …, isHtml: true)`. The recipient is the same one `DailyReport` uses.
5. Logging: an information line with the match count, and one line per match (`{Date} {Home} {HomeScore}-{AwayScore} {Away} [{Status}]`).
   On parse failure, a warning with the first 500 characters of the raw text.

## Error handling

- Every outcome sends an email, so silence always means the function didn't run, never that it ran and found nothing.
- An HTTP error or timeout from Gemini is logged with the status code and body, and produces the FAILED email. There is no retry: the job runs weekly
  and can be re-run by hand from the Azure portal or with `POST /admin/functions/WeeklyMatchSearch`.
- `EmailSMTPService` already catches and logs send failures.
- The top-level `try/catch` in `Run` logs any unexpected exception, matching `SquadSelector`.

## Configuration

- No new settings. Uses the existing `GEMINI_API_KEY`, `SMTP_SENDER` and `SMTP_PASSWORD`.

## Verification

There's no automated test suite in the repo, so verification is:

1. `dotnet build squad-func` builds cleanly with no new warnings.
2. Manual run: `func start` in `squad-func`, then `curl -X POST http://localhost:7071/admin/functions/WeeklyMatchSearch -H "Content-Type: application/json" -d '{}'`.
   Check the logs show parsed matches and the email arrives with a correct table and source links.
   Spot-check two or three scores against BBC Sport / premierleague.com.

## Documentation updates

- `squad-func/README.md`: add `WeeklyMatchSearch` to "Current Functions".
- `squad-func/AGENTS.md`: add it to "Core Functions & Responsibilities".
- Root `CLAUDE.md`: add it to the squad-func "Key functions" list and `weekly-results-prompt.md` to `prompts/`.
- `squad-func/CHANGELOG.md` → `Added`. Root `CHANGELOG.md` → `Added` (one line pointing to the subproject changelog).

## Notes

- `squad-func.csproj` targets `net10.0`, while `CLAUDE.md` and `AGENTS.md` say .NET 8. Not changed here, just noted.
