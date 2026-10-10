# Weekly Premier League Results Search Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A new `squad-func` timer function runs every Tuesday at 08:00 UTC. It asks Gemini, grounded with Google Search, for every Premier League match played in the previous 7 days, and emails the results as an HTML table.

**Architecture:**
- `WeeklyMatchSearch` (timer) works out the date window and calls the new `GeminiService.GetWeeklyResultsAsync`, which turns on the `google_search` tool.
- A pure `WeeklyResultsParser` turns Gemini's response envelope into a `WeeklyResultsResponse` holding the raw text, the parsed results (or null) and the grounding sources.
- A pure `WeeklyResultsEmail.Build` turns that into a subject and HTML body for every outcome: results, empty, unparsed, failed.
- No DB writes and no blob storage.

**Tech Stack:** .NET 10 (`net10.0`) Azure Functions isolated worker, `System.Text.Json`, the Gemini `v1beta` `generateContent` REST API, and the existing `EmailSMTPService` (Gmail SMTP).

**Spec:** `docs/superpowers/specs/2026-10-09-weekly-premier-league-results-design.md`

## Global Constraints

- No database writes, and no `SquadContext` dependency in the new function.
- No blob storage. The `playersname`-only container rule in `CLAUDE.md` / `squad-func/AGENTS.md` stays as it is.
- Timer schedule `0 0 8 * * 2` (Tuesdays 08:00 UTC). Window: `from = today − 7 days`, `to = today − 1 day`, both inclusive, `today = DateOnly.FromDateTime(DateTime.UtcNow)`.
- League constant: `"English Premier League"`. It is not configurable.
- Recipient: `"srafferty89@gmail.com"`, the same address `DailyReport` uses. No new app settings. Uses the existing `GEMINI_API_KEY`, `SMTP_SENDER` and `SMTP_PASSWORD`.
- Gemini: the existing `_agentModel` (`gemini-3.1-flash-lite`), `tools: [{ "google_search": {} }]`, a 120-second `CancellationTokenSource`. **Do not** set `responseMimeType`.
- Email subject base: `Premier League Results — {from:dd MMM} to {to:dd MMM yyyy}` (invariant culture). Suffixes ` (unparsed)` and ` (FAILED)`.
- Every Gemini-supplied string rendered into HTML goes through `WebUtility.HtmlEncode`.
- Existing `GeminiService` callers (`GenerateContentAsync`, `GetPlayerPhotoPrompt`) must behave exactly as before.
- Changelogs: `squad-func/CHANGELOG.md` and root `CHANGELOG.md` under `### Added`. The clean build gate is `dotnet build squad-func` with 0 warnings and 0 errors (that is the baseline).
- Commit messages end with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Review Focus

1. **Gemini wraps the JSON in ```` ```json ```` fences, or adds prose / a "Data retrieved from BBC Sport" line before or after it.** It should still parse. Pinned in Task 1, check `fenced json with surrounding prose`.
2. **Postponed or abandoned matches come back with `null` scores, or scores come back as strings (`"2"`).** They should parse, and the email shows the status and `–`, never a made-up `0–0`. Pinned in Task 1, `string and null scores`, and Task 2, `postponed shows status not 0-0`.
3. **Gemini includes matches outside the window**, e.g. the Tuesday of the run or the weekend before. Those rows are left out of the table and the email says how many were left out. A date with a time part still counts as in-window. Pinned in Task 2, `out-of-window matches left out` and `datetime-shaped date counts`.
4. **Team names with `&` (Brighton & Hove Albion), or raw text containing markup, or a non-http source URI.** The email must stay valid HTML and never link a `javascript:` URI. Pinned in Task 2, `ampersand encoded`, `raw text encoded`, `non-http source not linked`.
5. **A blocked prompt (`promptFeedback.blockReason`), an empty `candidates`, or a non-JSON response body (e.g. an HTML 502 page).** No exception: the result is "unparsed" with empty raw text. Pinned in Task 1, `blocked prompt`, `non-json envelope`, `matches null becomes empty`.

## Verification Harness (read before Task 1)

The repo has no test project (`CLAUDE.md`: the verification bar is a clean build). The pure units are checked with **.NET 10 file-based apps** that `#:project`-reference `squad-func`. This was confirmed to work in this repo on SDK 10.0.101. These check scripts are **not committed**. They live outside the repo in:

```
CHECKS=/private/tmp/claude-501/-Users-seanrafferty-Documents-development-repos-SquadComplete/198f89af-1b8e-4d6c-9df3-27504adac2ed/scratchpad/weekly-checks
```

If that directory is unavailable, use any empty directory **outside** the repo and keep the same absolute `#:project` path.
Run a check with `cd "$CHECKS" && dotnet run <file>.cs`.
- The run prints `PASS`/`FAIL` lines and exits non-zero if anything fails.
- MSBuild prints a pre-existing `MSB3277` warning about `Microsoft.EntityFrameworkCore.Relational` version conflicts when a file-based app references `squad-func`. Ignore it. It doesn't appear in `dotnet build squad-func`.

## File Structure

| File | Status | Responsibility |
|---|---|---|
| `squad-func/Models/WeeklyResults.cs` | Create | DTOs: `WeeklyResults`, `WeeklyMatch`, `GroundingSource`, `WeeklyResultsResponse` |
| `squad-func/Services/WeeklyResultsParser.cs` | Create | Pure: Gemini envelope JSON → `WeeklyResultsResponse` (text, parsed results, sources) |
| `squad-func/Models/WeeklyResultsEmail.cs` | Create | Pure: `(from, to, response?)` → `(Subject, Body)` HTML email |
| `squad-func/prompts/weekly-results-prompt.md` | Create | Gemini prompt with `{LEAGUE}`, `{FROM_DATE}`, `{TO_DATE}` |
| `squad-func/Services/GeminiService.cs` | Modify | Add `GetWeeklyResultsAsync`; `BuildBaseRequestBody` gets `useGoogleSearch` |
| `squad-func/squad-func.csproj` | Modify | Copy the new prompt to output |
| `squad-func/WeeklyMatchSearch.cs` | Create | Timer function: window → Gemini → log → email |
| `squad-func/README.md`, `squad-func/AGENTS.md`, `CLAUDE.md` | Modify | Document the function and prompt |
| `squad-func/CHANGELOG.md`, `CHANGELOG.md` | Modify | `### Added` entries |

---

### Task 1: Result models and Gemini response parser

**Files:**
- Create: `squad-func/Models/WeeklyResults.cs`
- Create: `squad-func/Services/WeeklyResultsParser.cs`
- Check (uncommitted): `$CHECKS/checks-parser.cs`

**Interfaces:**
- Consumes: nothing.
- Produces:
  - `namespace squad_func.Models`:
    - `class WeeklyResults { string? League; string? FromDate; string? ToDate; List<WeeklyMatch> Matches }`
    - `class WeeklyMatch { string? Date; string? HomeTeam; string? AwayTeam; int? HomeScore; int? AwayScore; string? Status }`
    - `record GroundingSource(string Title, string Uri)`
    - `class WeeklyResultsResponse { string RawText; WeeklyResults? Results; List<GroundingSource> Sources }` (init-only)
  - `namespace squad_func.Services`:
    - `static class WeeklyResultsParser`
      - `public static WeeklyResultsResponse FromGeminiResponse(string responseJson)`, which never throws.
      - `public static WeeklyResults? ParseResults(string text)`, which returns `null` when there's no parseable JSON object.

- [ ] **Step 1: Write the failing check script**

Create `$CHECKS/checks-parser.cs`:

```csharp
#:project /Users/seanrafferty/Documents/development/repos/SquadComplete/squad-func/squad-func.csproj
using System.Text.Json;
using squad_func.Services;

int failures = 0;
void Check(string name, bool ok)
{
    Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}");
    if (!ok) failures++;
}

// Wraps model text in a minimal Gemini generateContent envelope, escaping it properly.
string Envelope(string text) => JsonSerializer.Serialize(new
{
    candidates = new[] { new { content = new { parts = new[] { new { text } }, role = "model" } } }
});

const string Json = """
{"league":"English Premier League","from_date":"2026-10-06","to_date":"2026-10-12",
 "matches":[{"date":"2026-10-11","home_team":"Arsenal","away_team":"Chelsea","home_score":2,"away_score":1,"status":"FT"}]}
""";

var plain = WeeklyResultsParser.FromGeminiResponse(Envelope(Json));
Check("plain json parses", plain.Results?.Matches.Count == 1
    && plain.Results.Matches[0].HomeTeam == "Arsenal"
    && plain.Results.Matches[0].AwayTeam == "Chelsea"
    && plain.Results.Matches[0].HomeScore == 2
    && plain.Results.Matches[0].AwayScore == 1
    && plain.Results.Matches[0].Status == "FT"
    && plain.Results.Matches[0].Date == "2026-10-11");
Check("raw text kept", plain.RawText.Contains("\"home_team\":\"Arsenal\""));

var fenced = WeeklyResultsParser.FromGeminiResponse(
    Envelope("Here are the results:\n```json\n" + Json + "\n```\nData retrieved from BBC Sport."));
Check("fenced json with surrounding prose", fenced.Results?.Matches.Count == 1);

var scores = WeeklyResultsParser.ParseResults("""
{"matches":[
  {"date":"2026-10-11","home_team":"Spurs","away_team":"Everton","home_score":"3","away_score":"0","status":"FT"},
  {"date":"2026-10-12","home_team":"Leeds","away_team":"Fulham","home_score":null,"away_score":null,"status":"P-P"}]}
""");
Check("string and null scores", scores?.Matches.Count == 2
    && scores.Matches[0].HomeScore == 3 && scores.Matches[0].AwayScore == 0
    && scores.Matches[1].HomeScore == null && scores.Matches[1].AwayScore == null
    && scores.Matches[1].Status == "P-P");

var empty = WeeklyResultsParser.FromGeminiResponse(Envelope("""{"league":"English Premier League","matches":[]}"""));
Check("empty matches parses to zero", empty.Results != null && empty.Results.Matches.Count == 0);

var nullMatches = WeeklyResultsParser.ParseResults("""{"league":"English Premier League","matches":null}""");
Check("matches null becomes empty", nullMatches != null && nullMatches.Matches != null && nullMatches.Matches.Count == 0);

var prose = WeeklyResultsParser.FromGeminiResponse(Envelope("Sorry, I could not find any results."));
Check("non-json text is unparsed but kept", prose.Results == null && prose.RawText == "Sorry, I could not find any results.");

var blocked = WeeklyResultsParser.FromGeminiResponse("""{"promptFeedback":{"blockReason":"SAFETY"}}""");
Check("blocked prompt", blocked.Results == null && blocked.RawText == "" && blocked.Sources.Count == 0);

var html = WeeklyResultsParser.FromGeminiResponse("<html><body>502 Bad Gateway</body></html>");
Check("non-json envelope", html.Results == null && html.RawText == "");

var thought = WeeklyResultsParser.FromGeminiResponse("""
{"candidates":[{"content":{"parts":[{"text":"thinking about { braces","thought":true},{"text":"{\"matches\":[]}"}]}}]}
""");
Check("thought parts skipped", thought.Results != null && !thought.RawText.Contains("thinking"));

var grounded = WeeklyResultsParser.FromGeminiResponse("""
{"candidates":[{"content":{"parts":[{"text":"{\"matches\":[]}"}]},
  "groundingMetadata":{"groundingChunks":[
    {"web":{"uri":"https://www.bbc.co.uk/sport","title":"bbc.co.uk"}},
    {"web":{"uri":"https://www.bbc.co.uk/sport","title":"bbc.co.uk"}},
    {"web":{"title":"no uri"}},
    {"web":{"uri":"https://www.premierleague.com"}}]}}]}
""");
Check("grounding sources deduped and titled", grounded.Sources.Count == 2
    && grounded.Sources[0] == new squad_func.Models.GroundingSource("bbc.co.uk", "https://www.bbc.co.uk/sport")
    && grounded.Sources[1].Title == "https://www.premierleague.com");

Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILED");
return failures == 0 ? 0 : 1;
```

- [ ] **Step 2: Run it to verify it fails**

Run: `cd "$CHECKS" && dotnet run checks-parser.cs`
Expected: build failure with `error CS0234: The type or namespace name 'WeeklyResultsParser' does not exist` (or `CS0246`).

- [ ] **Step 3: Create the models**

Create `squad-func/Models/WeeklyResults.cs`:

```csharp
namespace squad_func.Models;

// Shapes for the weekly Premier League results search (WeeklyMatchSearch).
// WeeklyResults / WeeklyMatch mirror the JSON that prompts/weekly-results-prompt.md asks Gemini for
// (snake_case on the wire). Dates are kept as strings so one odd value doesn't fail the whole parse.

public class WeeklyResults
{
    public string? League { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public List<WeeklyMatch> Matches { get; set; } = [];
}

public class WeeklyMatch
{
    public string? Date { get; set; }
    public string? HomeTeam { get; set; }
    public string? AwayTeam { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public string? Status { get; set; }
}

public record GroundingSource(string Title, string Uri);

public class WeeklyResultsResponse
{
    /// <summary>The model's text output (all non-thought parts joined). Empty when Gemini returned none.</summary>
    public string RawText { get; init; } = string.Empty;

    /// <summary>Parsed results, or null when <see cref="RawText"/> held no parseable JSON object.</summary>
    public WeeklyResults? Results { get; init; }

    /// <summary>Web sources Gemini's Google Search grounding reported, de-duplicated by URI.</summary>
    public List<GroundingSource> Sources { get; init; } = [];
}
```

- [ ] **Step 4: Create the parser**

Create `squad-func/Services/WeeklyResultsParser.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using squad_func.Models;

namespace squad_func.Services;

/// <summary>
/// Turns a raw Gemini generateContent response into a <see cref="WeeklyResultsResponse"/>.
/// Has no I/O so it can be checked without calling Gemini; never throws on bad input.
/// </summary>
public static class WeeklyResultsParser
{
    private static readonly JsonSerializerOptions _resultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    /// <summary>
    /// Extracts the model text and grounding sources from a Gemini response envelope and parses the text.
    /// A blocked prompt, missing candidates or a non-JSON body yields empty text and null results.
    /// </summary>
    public static WeeklyResultsResponse FromGeminiResponse(string responseJson)
    {
        string rawText = string.Empty;
        List<GroundingSource> sources = [];
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("candidates", out var candidates)
                && candidates.ValueKind == JsonValueKind.Array
                && candidates.GetArrayLength() > 0
                && candidates[0].ValueKind == JsonValueKind.Object)
            {
                rawText = ExtractText(candidates[0]);
                sources = ExtractSources(candidates[0]);
            }
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // Not a well-formed Gemini envelope; keep whatever was extracted so the email reports it as unparsed.
        }

        return new WeeklyResultsResponse
        {
            RawText = rawText,
            Results = ParseResults(rawText),
            Sources = sources
        };
    }

    /// <summary>
    /// Parses the JSON object in the model's text, ignoring any code fences or prose around it
    /// (everything before the first '{' and after the last '}'). Returns null if there is none.
    /// </summary>
    public static WeeklyResults? ParseResults(string text)
    {
        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        try
        {
            var results = JsonSerializer.Deserialize<WeeklyResults>(text[start..(end + 1)], _resultOptions);
            if (results == null)
            {
                return null;
            }
            results.Matches ??= [];
            return results;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ExtractText(JsonElement candidate)
    {
        if (!candidate.TryGetProperty("content", out var content)
            || content.ValueKind != JsonValueKind.Object
            || !content.TryGetProperty("parts", out var parts)
            || parts.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var texts = parts.EnumerateArray()
            .Where(p => p.ValueKind == JsonValueKind.Object)
            .Where(p => !(p.TryGetProperty("thought", out var thought) && thought.ValueKind == JsonValueKind.True))
            .Select(p => p.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String ? text.GetString() : null)
            .Where(t => t != null);

        return string.Concat(texts).Trim();
    }

    private static List<GroundingSource> ExtractSources(JsonElement candidate)
    {
        if (!candidate.TryGetProperty("groundingMetadata", out var metadata)
            || metadata.ValueKind != JsonValueKind.Object
            || !metadata.TryGetProperty("groundingChunks", out var chunks)
            || chunks.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        List<GroundingSource> sources = [];
        foreach (var chunk in chunks.EnumerateArray())
        {
            if (chunk.ValueKind != JsonValueKind.Object
                || !chunk.TryGetProperty("web", out var web)
                || web.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            string? uri = web.TryGetProperty("uri", out var u) && u.ValueKind == JsonValueKind.String ? u.GetString() : null;
            if (string.IsNullOrWhiteSpace(uri))
            {
                continue;
            }

            string? title = web.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
            sources.Add(new GroundingSource(string.IsNullOrWhiteSpace(title) ? uri : title, uri));
        }

        return sources.DistinctBy(s => s.Uri).ToList();
    }
}
```

- [ ] **Step 5: Run the check to verify it passes**

Run: `cd "$CHECKS" && dotnet run checks-parser.cs`
Expected: every line `PASS`, then `ALL PASSED`, with exit code 0.

- [ ] **Step 6: Build gate**

Run: `cd /Users/seanrafferty/Documents/development/repos/SquadComplete && dotnet build squad-func`
Expected: `0 Warning(s)`, `0 Error(s)`.

- [ ] **Step 7: Commit**

```bash
cd /Users/seanrafferty/Documents/development/repos/SquadComplete
git add squad-func/Models/WeeklyResults.cs squad-func/Services/WeeklyResultsParser.cs
git commit -m "feat(squad-func): add weekly results models and Gemini response parser

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 2: Results email builder

**Files:**
- Create: `squad-func/Models/WeeklyResultsEmail.cs`
- Check (uncommitted): `$CHECKS/checks-email.cs`

**Interfaces:**
- Consumes: `WeeklyResultsResponse`, `WeeklyResults`, `WeeklyMatch` and `GroundingSource` from Task 1 (`squad_func.Models`).
- Produces: `namespace squad_func.Models` → `static class WeeklyResultsEmail` with `public static (string Subject, string Body) Build(DateOnly from, DateOnly to, WeeklyResultsResponse? response)`.
  `response == null` means the Gemini call failed.

- [ ] **Step 1: Write the failing check script**

Create `$CHECKS/checks-email.cs`:

```csharp
#:project /Users/seanrafferty/Documents/development/repos/SquadComplete/squad-func/squad-func.csproj
using squad_func.Models;

int failures = 0;
void Check(string name, bool ok)
{
    Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}");
    if (!ok) failures++;
}

var from = new DateOnly(2026, 10, 6);
var to = new DateOnly(2026, 10, 12);
const string BaseSubject = "Premier League Results — 06 Oct to 12 Oct 2026";

WeeklyMatch M(string date, string home, string away, int? hs, int? aws, string status) =>
    new() { Date = date, HomeTeam = home, AwayTeam = away, HomeScore = hs, AwayScore = aws, Status = status };

WeeklyResultsResponse Parsed(params WeeklyMatch[] matches) =>
    new() { RawText = "{}", Results = new WeeklyResults { Matches = [.. matches] } };

var failed = WeeklyResultsEmail.Build(from, to, null);
Check("failed subject", failed.Subject == BaseSubject + " (FAILED)");
Check("failed body", failed.Body.Contains("Gemini search failed"));

var unparsed = WeeklyResultsEmail.Build(from, to, new WeeklyResultsResponse { RawText = "<script>alert(1)</script> no json" });
Check("unparsed subject", unparsed.Subject == BaseSubject + " (unparsed)");
Check("raw text encoded", unparsed.Body.Contains("&lt;script&gt;alert(1)&lt;/script&gt;") && !unparsed.Body.Contains("<script>"));

var results = WeeklyResultsEmail.Build(from, to, Parsed(
    M("2026-10-11", "Brighton & Hove Albion", "Chelsea", 1, 1, "FT"),
    M("2026-10-10", "Arsenal", "Tottenham Hotspur", 2, 0, "FT")));
Check("results subject", results.Subject == BaseSubject);
Check("ampersand encoded", results.Body.Contains("Brighton &amp; Hove Albion") && !results.Body.Contains("Brighton & Hove"));
Check("score rendered", results.Body.Contains("2–0") && results.Body.Contains("1–1"));
Check("sorted by date", results.Body.IndexOf("Arsenal") < results.Body.IndexOf("Brighton"));
Check("no left-out note when none excluded", !results.Body.Contains("Left out"));

var postponed = WeeklyResultsEmail.Build(from, to, Parsed(M("2026-10-12", "Leeds United", "Fulham", null, null, "P-P")));
Check("postponed shows status not 0-0", postponed.Body.Contains("P-P") && !postponed.Body.Contains("0–0") && postponed.Body.Contains("Leeds United"));

var outside = WeeklyResultsEmail.Build(from, to, Parsed(
    M("2026-10-05", "Old Weekend FC", "X", 1, 0, "FT"),
    M("2026-10-13", "Run Day FC", "Y", 0, 0, "FT"),
    M("2026-10-06", "First Day FC", "Z", 3, 2, "FT"),
    M("2026-10-12", "Last Day FC", "W", 0, 1, "FT")));
Check("out-of-window matches left out", !outside.Body.Contains("Old Weekend FC") && !outside.Body.Contains("Run Day FC")
    && outside.Body.Contains("First Day FC") && outside.Body.Contains("Last Day FC")
    && outside.Body.Contains("Left out 2 match(es) dated outside this window."));

var withTime = WeeklyResultsEmail.Build(from, to, Parsed(M("2026-10-11T15:00:00Z", "Timed FC", "V", 1, 0, "FT")));
Check("datetime-shaped date counts", withTime.Body.Contains("Timed FC") && !withTime.Body.Contains("Left out"));

var undated = WeeklyResultsEmail.Build(from, to, Parsed(M("last Saturday", "Undated FC", "U", 1, 0, "FT")));
Check("unparseable date kept", undated.Body.Contains("Undated FC"));

var none = WeeklyResultsEmail.Build(from, to, Parsed());
Check("empty week", none.Subject == BaseSubject && none.Body.Contains("No Premier League matches found for this window."));

var sourced = WeeklyResultsEmail.Build(from, to, new WeeklyResultsResponse
{
    RawText = "{}",
    Results = new WeeklyResults(),
    Sources = [new GroundingSource("bbc.co.uk", "https://www.bbc.co.uk/sport"), new GroundingSource("evil", "javascript:alert(1)")]
});
Check("http source linked", sourced.Body.Contains("href=\"https://www.bbc.co.uk/sport\"") && sourced.Body.Contains(">bbc.co.uk</a>"));
Check("non-http source not linked", !sourced.Body.Contains("href=\"javascript:") && sourced.Body.Contains("evil"));

Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILED");
return failures == 0 ? 0 : 1;
```

- [ ] **Step 2: Run it to verify it fails**

Run: `cd "$CHECKS" && dotnet run checks-email.cs`
Expected: build failure with `error CS0103: The name 'WeeklyResultsEmail' does not exist in the current context`.

- [ ] **Step 3: Implement the email builder**

Create `squad-func/Models/WeeklyResultsEmail.cs`. Notes:
- The page uses a `$$"""` raw string, so single `{ }` in the CSS are literal and `{{expr}}` interpolates.
- The visual style copies `DailyStats.ToHtml()`.

```csharp
using System.Globalization;
using System.Net;
using System.Text;

namespace squad_func.Models;

/// <summary>
/// Builds the WeeklyMatchSearch email. Every outcome produces an email so silence always means "didn't run":
/// results table, empty week, unparsed Gemini reply (raw text shown), or failed Gemini call (null response).
/// </summary>
public static class WeeklyResultsEmail
{
    public static (string Subject, string Body) Build(DateOnly from, DateOnly to, WeeklyResultsResponse? response)
    {
        string subject = $"Premier League Results — {Format(from, "dd MMM")} to {Format(to, "dd MMM yyyy")}";
        string window = $"{Format(from, "ddd dd MMM")} – {Format(to, "ddd dd MMM yyyy")}";

        if (response == null)
        {
            return ($"{subject} (FAILED)",
                Page(window, Banner("Gemini search failed. Check the WeeklyMatchSearch function logs.")));
        }

        if (response.Results == null)
        {
            string unparsed = Banner("Gemini's reply could not be parsed as JSON. Raw reply below.")
                + $"<pre class=\"raw\">{Encode(response.RawText)}</pre>"
                + SourcesHtml(response.Sources);
            return ($"{subject} (unparsed)", Page(window, unparsed));
        }

        var matches = response.Results.Matches
            .Where(m => IsInWindow(m, from, to))
            .OrderBy(m => m.Date, StringComparer.Ordinal)
            .ThenBy(m => m.HomeTeam, StringComparer.OrdinalIgnoreCase)
            .ToList();
        int excluded = response.Results.Matches.Count - matches.Count;

        var content = new StringBuilder();
        if (matches.Count == 0)
        {
            content.Append("<p class=\"empty\">No Premier League matches found for this window.</p>");
        }
        else
        {
            content.Append("<table class=\"results\"><tr><th>Date</th><th>Home</th><th class=\"score\">Score</th><th>Away</th><th>Status</th></tr>");
            foreach (var match in matches)
            {
                content.Append("<tr>")
                    .Append($"<td>{Encode(match.Date)}</td>")
                    .Append($"<td>{Encode(match.HomeTeam)}</td>")
                    .Append($"<td class=\"score\">{Score(match)}</td>")
                    .Append($"<td>{Encode(match.AwayTeam)}</td>")
                    .Append($"<td>{Encode(match.Status)}</td>")
                    .Append("</tr>");
            }
            content.Append("</table>");
        }

        if (excluded > 0)
        {
            content.Append($"<p class=\"note\">Left out {excluded} match(es) dated outside this window.</p>");
        }

        content.Append(SourcesHtml(response.Sources));
        return (subject, Page(window, content.ToString()));
    }

    /// <summary>Matches with an unreadable date are kept (and shown as given) rather than silently dropped.</summary>
    private static bool IsInWindow(WeeklyMatch match, DateOnly from, DateOnly to)
    {
        string? date = match.Date?.Trim();
        if (date == null || date.Length < 10
            || !DateOnly.TryParseExact(date[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
        {
            return true;
        }
        return day >= from && day <= to;
    }

    private static string Score(WeeklyMatch match) =>
        match.HomeScore.HasValue && match.AwayScore.HasValue ? $"{match.HomeScore}–{match.AwayScore}" : "–";

    private static string SourcesHtml(List<GroundingSource> sources)
    {
        if (sources.Count == 0)
        {
            return string.Empty;
        }

        var html = new StringBuilder("<div class=\"section-title\">Sources</div><ol class=\"sources\">");
        foreach (var source in sources)
        {
            bool linkable = source.Uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || source.Uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
            html.Append(linkable
                ? $"<li><a href=\"{Encode(source.Uri)}\">{Encode(source.Title)}</a></li>"
                : $"<li>{Encode(source.Title)}</li>");
        }
        return html.Append("</ol>").ToString();
    }

    private static string Banner(string message) => $"<div class=\"banner\">{Encode(message)}</div>";

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string Format(DateOnly date, string format) => date.ToString(format, CultureInfo.InvariantCulture);

    private static string Page(string window, string content) => $$"""
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Premier League Results</title>
<style>
  body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f6f8; color: #333333; margin: 0; padding: 24px 16px; }
  .container { max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.06); border: 1px solid #e5e7eb; }
  .header { background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%); color: #ffffff; padding: 28px 24px; text-align: center; }
  .header h1 { margin: 0 0 6px 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px; }
  .header p { margin: 0; font-size: 14px; opacity: 0.9; }
  .content { padding: 24px; }
  .section-title { font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.8px; color: #6b7280; margin: 20px 0 10px 0; padding-bottom: 6px; border-bottom: 1px solid #f3f4f6; }
  .results { width: 100%; border-collapse: collapse; font-size: 14px; }
  .results th { text-align: left; font-size: 12px; color: #6b7280; text-transform: uppercase; letter-spacing: 0.5px; padding: 8px 4px; border-bottom: 1px solid #e5e7eb; }
  .results td { padding: 10px 4px; border-bottom: 1px solid #f3f4f6; color: #111827; }
  .score { font-weight: 700; text-align: center; white-space: nowrap; }
  .banner { background-color: #fef3c7; color: #92400e; padding: 12px 16px; border-radius: 8px; font-size: 14px; }
  .note, .empty { color: #6b7280; font-size: 13px; }
  .raw { white-space: pre-wrap; word-break: break-word; background: #f9fafb; padding: 12px; border-radius: 8px; font-size: 12px; }
  .sources { font-size: 12px; padding-left: 18px; color: #4b5563; }
  .footer { background-color: #f9fafb; padding: 16px 24px; text-align: center; font-size: 12px; color: #9ca3af; border-top: 1px solid #f3f4f6; }
</style>
</head>
<body>
<div class="container">
  <div class="header">
    <h1>Premier League Results</h1>
    <p>{{window}}</p>
  </div>
  <div class="content">
{{content}}
  </div>
  <div class="footer">Generated by WeeklyMatchSearch using Gemini with Google Search grounding. Check sources before relying on these results.</div>
</div>
</body>
</html>
""";
}
```

- [ ] **Step 4: Run the check to verify it passes**

Run: `cd "$CHECKS" && dotnet run checks-email.cs`
Expected: every line `PASS`, then `ALL PASSED`, with exit code 0.

- [ ] **Step 5: Build gate**

Run: `cd /Users/seanrafferty/Documents/development/repos/SquadComplete && dotnet build squad-func`
Expected: `0 Warning(s)`, `0 Error(s)`.

- [ ] **Step 6: Commit**

```bash
cd /Users/seanrafferty/Documents/development/repos/SquadComplete
git add squad-func/Models/WeeklyResultsEmail.cs
git commit -m "feat(squad-func): add weekly results HTML email builder

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 3: Grounded Gemini call and prompt

**Files:**
- Create: `squad-func/prompts/weekly-results-prompt.md`
- Modify: `squad-func/Services/GeminiService.cs` (usings at lines 1–8, `BuildBaseRequestBody` at lines 59–82, new method after `GenerateContentAsync`)
- Modify: `squad-func/squad-func.csproj` (prompt `ItemGroup`, lines 30–33)

**Interfaces:**
- Consumes: `WeeklyResultsParser.FromGeminiResponse(string)` (Task 1), `WeeklyResultsResponse` (Task 1).
- Produces: `GeminiService.GetWeeklyResultsAsync(string league, DateOnly from, DateOnly to) : Task<WeeklyResultsResponse?>`, which returns `null` on an HTTP error, a timeout or an exception (logged).
  `GeminiService` is in the global namespace.

- [ ] **Step 1: Create the prompt**

Create `squad-func/prompts/weekly-results-prompt.md`:

```markdown
### Role
You are a specialised men's football (soccer) results agent. Use Google Search to find real, completed match results. Never rely on memory alone.

### Task
Find every {LEAGUE} match played between {FROM_DATE} and {TO_DATE} (both dates inclusive, UK time).

### Rules
- Use web search and trusted sources (BBC Sport, premierleague.com, Sky Sports, ESPN) to confirm every result.
- Include only matches whose kick-off date falls within the window. Exclude matches before {FROM_DATE} or after {TO_DATE}.
- Include postponed or abandoned fixtures that were scheduled within the window, with their status.
- Never invent a result. If you cannot confirm a score, set both scores to null.
- If no matches were played in the window, return an empty "matches" array.
- Use each club's common English name (e.g. "Manchester United", "Brighton & Hove Albion").

### Output
Reply with ONLY a single JSON object, no markdown fences and no other text, in exactly this shape:

{
  "league": "{LEAGUE}",
  "from_date": "{FROM_DATE}",
  "to_date": "{TO_DATE}",
  "matches": [
    {
      "date": "YYYY-MM-DD",
      "home_team": "String",
      "away_team": "String",
      "home_score": 0,
      "away_score": 0,
      "status": "FT"
    }
  ]
}

- "home_score" and "away_score" are integers, or null when the match was not completed.
- "status" is one of: "FT" (full time), "AET" (after extra time), "PEN" (decided on penalties), "P-P" (postponed), "ABD" (abandoned).
```

- [ ] **Step 2: Copy the prompt to the build output**

In `squad-func/squad-func.csproj`, directly after the existing block:

```xml
    <None Update="./prompts/agent-prompt.md">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
```

add:

```xml
    <None Update="./prompts/weekly-results-prompt.md">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
```

- [ ] **Step 3: Add the optional search tool to `BuildBaseRequestBody`**

In `squad-func/Services/GeminiService.cs`, replace the whole `BuildBaseRequestBody` method, its doc comment included, with the version below. The serializer already uses `WhenWritingNull`, so `tools` is omitted for existing callers. Their request body doesn't change.

```csharp
    /// <summary>
    /// Builds the base request body for the Gemini API.
    /// </summary>
    /// <param name="userPrompt">The user prompt.</param>
    /// <param name="useGoogleSearch">When true, enables Google Search grounding so the model can look up live data.
    /// Don't combine with a JSON responseMimeType; ask for JSON in the prompt instead.</param>
    /// <returns>The request body.</returns>
    private static object BuildBaseRequestBody(string userPrompt, bool useGoogleSearch = false)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = userPrompt }
                    }
                }
            },
            tools = useGoogleSearch ? new[] { new { google_search = new { } } } : null,
            generationConfig = new
            {
                temperature = 0.2,
                // Latency is directly proportional to the number of tokens generated.
                // Use the max_output_tokens parameter to restrict the length of the response
                // double the output tokens does not seem to have an impact on the response time
                maxOutputTokens = 12288
            }
        };
        return requestBody;
    }
```

- [ ] **Step 4: Add `GetWeeklyResultsAsync`**

At the top of `squad-func/Services/GeminiService.cs`, add these to the existing `using` lines:

```csharp
using System.Globalization;
using squad_func.Models;
using squad_func.Services;
```

Then, directly after the closing brace of `GenerateContentAsync`, insert:

```csharp
    /// <summary>
    /// Asks Gemini, grounded with Google Search, for every match in a league between two dates (inclusive).
    /// </summary>
    /// <param name="league">The league name, e.g. "English Premier League".</param>
    /// <param name="from">First day of the window.</param>
    /// <param name="to">Last day of the window.</param>
    /// <returns>The parsed response (results may be null if the reply wasn't valid JSON), or null if the call failed.</returns>
    public async Task<WeeklyResultsResponse?> GetWeeklyResultsAsync(string league, DateOnly from, DateOnly to)
    {
        try
        {
            string promptFilePath = Path.Combine(AppContext.BaseDirectory, "prompts/weekly-results-prompt.md");
            string template = await File.ReadAllTextAsync(promptFilePath);
            string prompt = template
                .Replace("{LEAGUE}", league)
                .Replace("{FROM_DATE}", from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                .Replace("{TO_DATE}", to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            var requestBody = BuildBaseRequestBody(prompt, useGoogleSearch: true);
            string json = JsonSerializer.Serialize(requestBody, _serializerOptions);

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_agentModel}:generateContent?key={_apiKey}";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            _logger.LogInformation("Sending weekly results request to Gemini API for {League} {From} to {To}...", league, from, to);
            HttpResponseMessage response = await _httpClient.PostAsync(url, content, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error HTTP {StatusCode}: {ErrorContent}", (int)response.StatusCode, errorContent);
                return null;
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            return WeeklyResultsParser.FromGeminiResponse(responseJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weekly results from Gemini: {Error}", ex.Message);
            return null;
        }
    }
```

- [ ] **Step 5: Build gate, and check the prompt reaches the output folder**

Run:
```bash
cd /Users/seanrafferty/Documents/development/repos/SquadComplete && dotnet build squad-func \
  && grep -c "{FROM_DATE}" squad-func/bin/Debug/net10.0/prompts/weekly-results-prompt.md
```
Expected: `0 Warning(s)`, `0 Error(s)`, then a count of at least `2`. That shows the prompt was copied with its placeholders intact.

- [ ] **Step 6: Re-run both earlier checks (regression)**

Run: `cd "$CHECKS" && dotnet run checks-parser.cs && dotnet run checks-email.cs`
Expected: `ALL PASSED` twice.

- [ ] **Step 7: Commit**

```bash
cd /Users/seanrafferty/Documents/development/repos/SquadComplete
git add squad-func/prompts/weekly-results-prompt.md squad-func/Services/GeminiService.cs squad-func/squad-func.csproj
git commit -m "feat(squad-func): add Google Search grounded weekly results Gemini call

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 4: `WeeklyMatchSearch` timer function, docs and changelogs

**Files:**
- Create: `squad-func/WeeklyMatchSearch.cs`
- Modify: `squad-func/README.md` (the "Current Functions" list, between `RecordRequest` and `SquadSelector`)
- Modify: `squad-func/AGENTS.md` (section 2, "Core Functions & Responsibilities")
- Modify: `CLAUDE.md` (squad-func "Key functions" bullet and `prompts/` bullet)
- Modify: `squad-func/CHANGELOG.md`, `CHANGELOG.md` (under `## [Unreleased]` → `### Added`)
- Check (uncommitted): `$CHECKS/checks-window.cs`

**Interfaces:**
- Consumes:
  - `GeminiService.GetWeeklyResultsAsync(string, DateOnly, DateOnly) : Task<WeeklyResultsResponse?>` (Task 3)
  - `WeeklyResultsEmail.Build(DateOnly, DateOnly, WeeklyResultsResponse?) : (string Subject, string Body)` (Task 2)
  - `EmailSMTPService.SendEmail(string recipient, string subject, string body, bool isHtml = false)` (existing, global namespace)
- Produces: `Squad.Function.WeeklyMatchSearch`, with `public static (DateOnly From, DateOnly To) GetWindow(DateTime utcNow)` and Azure Function `"WeeklyMatchSearch"`.

- [ ] **Step 1: Write the failing window check**

Create `$CHECKS/checks-window.cs`:

```csharp
#:project /Users/seanrafferty/Documents/development/repos/SquadComplete/squad-func/squad-func.csproj
using Squad.Function;

int failures = 0;
void Check(string name, bool ok)
{
    Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}");
    if (!ok) failures++;
}

// 2026-10-13 is a Tuesday: the window is the previous Tuesday through Monday.
var tuesday = WeeklyMatchSearch.GetWindow(new DateTime(2026, 10, 13, 8, 0, 0, DateTimeKind.Utc));
Check("tuesday run covers tue-mon", tuesday == (new DateOnly(2026, 10, 6), new DateOnly(2026, 10, 12)));

var lateNight = WeeklyMatchSearch.GetWindow(new DateTime(2026, 10, 13, 23, 59, 59, DateTimeKind.Utc));
Check("time of day ignored", lateNight == (new DateOnly(2026, 10, 6), new DateOnly(2026, 10, 12)));

var newYear = WeeklyMatchSearch.GetWindow(new DateTime(2026, 1, 6, 8, 0, 0, DateTimeKind.Utc));
Check("crosses year boundary", newYear == (new DateOnly(2025, 12, 30), new DateOnly(2026, 1, 5)));

var manualFriday = WeeklyMatchSearch.GetWindow(new DateTime(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc));
Check("manual run on another day covers previous 7 days", manualFriday == (new DateOnly(2026, 10, 2), new DateOnly(2026, 10, 8)));

Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILED");
return failures == 0 ? 0 : 1;
```

- [ ] **Step 2: Run it to verify it fails**

Run: `cd "$CHECKS" && dotnet run checks-window.cs`
Expected: build failure with `error CS0103: The name 'WeeklyMatchSearch' does not exist in the current context`.

- [ ] **Step 3: Implement the function**

Create `squad-func/WeeklyMatchSearch.cs`:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using squad_func.Models;

namespace Squad.Function;

/// <summary>
/// Weekly Premier League results search. Asks Gemini (grounded with Google Search) for every match played
/// in the previous 7 days and emails the results. Read-only: nothing is written to the database or blob storage.
/// Schedule - Tuesdays 08:00 UTC, after the Monday night game.
/// </summary>
public class WeeklyMatchSearch(GeminiService geminiService, EmailSMTPService emailService, ILogger<WeeklyMatchSearch> logger)
{
    private const string League = "English Premier League";
    private const string Recipient = "srafferty89@gmail.com";

    private readonly GeminiService _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
    private readonly EmailSMTPService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly ILogger<WeeklyMatchSearch> _logger = logger;

    [Function("WeeklyMatchSearch")]
    public async Task Run([TimerTrigger("0 0 8 * * 2")] TimerInfo myTimer)
    {
        try
        {
            var (from, to) = GetWindow(DateTime.UtcNow);
            _logger.LogInformation("WeeklyMatchSearch searching {League} from {From} to {To}", League, from, to);

            var response = await _geminiService.GetWeeklyResultsAsync(League, from, to);
            LogResults(response);

            var (subject, body) = WeeklyResultsEmail.Build(from, to, response);
            _emailService.SendEmail(recipient: Recipient, subject: subject, body: body, isHtml: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during weekly match search.");
        }
    }

    /// <summary>
    /// The 7 days before the run date, inclusive: run on a Tuesday, it covers the previous Tuesday through Monday.
    /// </summary>
    public static (DateOnly From, DateOnly To) GetWindow(DateTime utcNow)
    {
        var today = DateOnly.FromDateTime(utcNow);
        return (today.AddDays(-7), today.AddDays(-1));
    }

    private void LogResults(WeeklyResultsResponse? response)
    {
        if (response == null)
        {
            _logger.LogError("Gemini weekly results search failed");
            return;
        }

        if (response.Results == null)
        {
            string preview = response.RawText.Length > 500 ? response.RawText[..500] : response.RawText;
            _logger.LogWarning("Gemini weekly results could not be parsed. Raw reply starts: {Preview}", preview);
            return;
        }

        _logger.LogInformation("Gemini returned {MatchCount} matches from {SourceCount} sources",
            response.Results.Matches.Count, response.Sources.Count);
        foreach (var match in response.Results.Matches)
        {
            _logger.LogInformation("{Date} {HomeTeam} {HomeScore}-{AwayScore} {AwayTeam} [{Status}]",
                match.Date, match.HomeTeam, match.HomeScore, match.AwayScore, match.AwayTeam, match.Status);
        }
    }
}
```

- [ ] **Step 4: Run the window check to verify it passes, plus the regression checks**

Run: `cd "$CHECKS" && dotnet run checks-window.cs && dotnet run checks-parser.cs && dotnet run checks-email.cs`
Expected: `ALL PASSED` three times.

- [ ] **Step 5: Update `squad-func/README.md`**

In the "Current Functions" list, insert this between the `RecordRequest` entry and the `SquadSelector` entry:

```markdown
- **WeeklyMatchSearch**:
  - **Trigger**: `0 0 8 * * 2` (Weekly on Tuesday at 08:00 UTC)
  - **Description**: Asks Gemini (with Google Search grounding, prompt `prompts/weekly-results-prompt.md`) for every English Premier League match played in the previous 7 days (Tuesday–Monday) and emails the date, teams, score and status as an HTML table, plus the web sources Gemini cited. Read-only: nothing is written to the database or blob storage. Every run sends an email (results, empty week, `(unparsed)` with Gemini's raw reply, or `(FAILED)`).
```

- [ ] **Step 6: Update `squad-func/AGENTS.md`**

In section 2, "Core Functions & Responsibilities", add after the `RecordRequest.cs` bullet:

```markdown
- **`WeeklyMatchSearch.cs`**: Tuesday cron that asks Gemini (Google Search grounded) for the previous 7 days of Premier League results and emails them. Read-only, no DB or blob writes. Parsing lives in `Services/WeeklyResultsParser.cs` and the email in `Models/WeeklyResultsEmail.cs`, both pure and free of I/O.
```

- [ ] **Step 7: Update root `CLAUDE.md`**

In the `### squad-func (background jobs)` section:
- In the "Key functions" bullet, change `` `GetPlayerImage.cs`/`DailyReport.cs`. `` to `` `GetPlayerImage.cs`/`DailyReport.cs`, `WeeklyMatchSearch.cs` (Tuesday Gemini search for the past week's Premier League results, emailed; no DB/blob writes). ``
- Change `` - `prompts/`: Gemini prompt templates (`agent-prompt.md`, `playername-prompt.md`). `` to `` - `prompts/`: Gemini prompt templates (`agent-prompt.md`, `playername-prompt.md`, `weekly-results-prompt.md`). ``

- [ ] **Step 8: Update changelogs**

In `squad-func/CHANGELOG.md`, under `## [Unreleased]` → `### Added`, add as the first bullet:

```markdown
- Added `WeeklyMatchSearch`, a Tuesday 08:00 UTC timer (`0 0 8 * * 2`) that asks Gemini, grounded with Google Search, for every English Premier League match played in the previous 7 days and emails the results (date, teams, score, status, cited sources) as an HTML table. Read-only: no database or blob writes. Matches Gemini returns outside the window are left out and counted in the email. Every run sends an email, including `(unparsed)` (raw reply shown) and `(FAILED)` variants. Adds `GeminiService.GetWeeklyResultsAsync`, an opt-in `useGoogleSearch` flag on the Gemini request builder (existing calls unchanged), `prompts/weekly-results-prompt.md`, `Services/WeeklyResultsParser.cs`, `Models/WeeklyResults.cs` and `Models/WeeklyResultsEmail.cs`.
```

In root `CHANGELOG.md`, under `## [Unreleased]`, add `### Added` if it isn't there yet. Then add as its first bullet:

```markdown
- `squad-func`: added `WeeklyMatchSearch`, a Tuesday Gemini (Google Search grounded) search for the past week's Premier League results, emailed to the administrator. See `squad-func/CHANGELOG.md`.
```

- [ ] **Step 9: Build gate**

Run: `cd /Users/seanrafferty/Documents/development/repos/SquadComplete && dotnet build squad-func`
Expected: `0 Warning(s)`, `0 Error(s)`.

- [ ] **Step 10: Commit**

```bash
cd /Users/seanrafferty/Documents/development/repos/SquadComplete
git add squad-func/WeeklyMatchSearch.cs squad-func/README.md squad-func/AGENTS.md CLAUDE.md squad-func/CHANGELOG.md CHANGELOG.md
git commit -m "feat(squad-func): add WeeklyMatchSearch Tuesday Premier League results email

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

- [ ] **Step 11: Manual end-to-end run (human step: real Gemini key and real email)**

This calls the paid Gemini API and sends a real email. It needs `GEMINI_API_KEY`, `SMTP_SENDER` and `SMTP_PASSWORD` in `squad-func/local.settings.json`, and the `AzureWebJobsStorage` the timer host already uses locally (e.g. Azurite). **Ask the human partner to run this, or get explicit permission first.**

```bash
cd /Users/seanrafferty/Documents/development/repos/SquadComplete/squad-func && func start
# in a second terminal:
curl -X POST http://localhost:7071/admin/functions/WeeklyMatchSearch -H "Content-Type: application/json" -d '{}'
```

Expected:
- `func` logs show `Gemini returned N matches from M sources`, then one line per match.
- An email arrives with subject `Premier League Results — 02 Oct to 08 Oct 2026` (for a run on 2026-10-09) and a results table, plus source links.
- Spot-check two or three scores against BBC Sport / premierleague.com.

If Gemini returns HTTP 400 saying the search tool isn't supported for `gemini-3.1-flash-lite`, stop and report it to the human partner. Don't change the model on your own.
