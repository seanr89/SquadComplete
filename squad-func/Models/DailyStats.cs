

public class DailyStats
{
    public DateTime Date { get; set; } = DateTime.Now.AddDays(-1);
    public int TotalTeams { get; set; } = 0;
    public int ActiveTeams { get; set; } = 0;
    public int TotalMatches { get; set; } = 0;
    public int TotalPlayers { get; set; } = 0;
    public int TotalUserSquads { get; set; } = 0;
    public int TotalGameRecords { get; set; } = 0;
    public int AIFixtureCount { get; set; } = 0;
    public int PlayersMissingPhotos { get; set; } = 0;

    public int TotalFeedbacks { get; set; } = 0;
    public int TotalEvents { get; set; } = 0;
    public DateTime? LatestEventDate { get; set; }

    #region Storage Info

    public int TotalSingleFixtureRecords { get; set; } = 0;
    public int TotalTeamRecords { get; set; } = 0;

    #endregion


    public override string ToString()
    {
        var latestEventStr = LatestEventDate.HasValue 
            ? LatestEventDate.Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") 
            : "None";

        return $@"Squad Daily Report — {Date:dddd, dd MMMM yyyy}
==================================================

[ Core Stats ]
  Total Teams:                 {TotalTeams:N0}
  Active Teams:                {ActiveTeams:N0}
  Total Matches:               {TotalMatches:N0}
  Total Players:               {TotalPlayers:N0}

[ User & Game Activity ]
  Total User Squads:           {TotalUserSquads:N0}
  Total Game Records:          {TotalGameRecords:N0}
  Total Events:                {TotalEvents:N0} (Latest: {latestEventStr})
  Total Feedbacks:             {TotalFeedbacks:N0}

[ AI & Data Quality ]
  AI Fixtures:                 {AIFixtureCount:N0}
  Players Missing Photos:      {PlayersMissingPhotos:N0}

[ Storage / Azure Blobs ]
  Total Team Records:          {TotalTeamRecords:N0}
  Total Single Fixture Records:{TotalSingleFixtureRecords:N0}
";
    }

    public string ToHtml()
    {
        return $@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>Squad Daily Report</title>
<style>
  body {{
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
    background-color: #f4f6f8;
    color: #333333;
    margin: 0;
    padding: 24px 16px;
  }}
  .container {{
    max-width: 600px;
    margin: 0 auto;
    background: #ffffff;
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 4px 12px rgba(0,0,0,0.06);
    border: 1px solid #e5e7eb;
  }}
  .header {{
    background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%);
    color: #ffffff;
    padding: 28px 24px;
    text-align: center;
  }}
  .header h1 {{
    margin: 0 0 6px 0;
    font-size: 24px;
    font-weight: 700;
    letter-spacing: -0.5px;
  }}
  .header p {{
    margin: 0;
    font-size: 14px;
    opacity: 0.9;
  }}
  .content {{
    padding: 24px;
  }}
  .section-title {{
    font-size: 12px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.8px;
    color: #6b7280;
    margin: 20px 0 10px 0;
    padding-bottom: 6px;
    border-bottom: 1px solid #f3f4f6;
  }}
  .section-title:first-child {{
    margin-top: 0;
  }}
  .stat-grid {{
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 8px;
  }}
  .stat-grid tr {{
    border-bottom: 1px solid #f9fafb;
  }}
  .stat-label {{
    padding: 10px 0;
    font-size: 14px;
    color: #4b5563;
  }}
  .stat-value {{
    padding: 10px 0;
    font-size: 15px;
    font-weight: 600;
    text-align: right;
    color: #111827;
  }}
  .badge {{
    display: inline-block;
    padding: 2px 8px;
    border-radius: 9999px;
    font-size: 13px;
    font-weight: 600;
  }}
  .badge-warning {{
    background-color: #fef3c7;
    color: #92400e;
  }}
  .badge-neutral {{
    background-color: #f3f4f6;
    color: #1f2937;
  }}
  .footer {{
    background-color: #f9fafb;
    padding: 16px 24px;
    text-align: center;
    font-size: 12px;
    color: #9ca3af;
    border-top: 1px solid #f3f4f6;
  }}
</style>
</head>
<body>
<div class=""container"">
  <div class=""header"">
    <h1>Squad Daily Report</h1>
    <p>{Date:dddd, dd MMMM yyyy}</p>
  </div>
  <div class=""content"">
    <div class=""section-title"">Core Metrics</div>
    <table class=""stat-grid"">
      <tr>
        <td class=""stat-label"">Total Teams</td>
        <td class=""stat-value"">{TotalTeams:N0}</td>
      </tr>
      <tr>
        <td class=""stat-label"">Active Teams</td>
        <td class=""stat-value""><span class=""badge badge-neutral"">{ActiveTeams:N0}</span></td>
      </tr>
      <tr>
        <td class=""stat-label"">Total Matches</td>
        <td class=""stat-value"">{TotalMatches:N0}</td>
      </tr>
      <tr>
        <td class=""stat-label"">Total Players</td>
        <td class=""stat-value"">{TotalPlayers:N0}</td>
      </tr>
    </table>

    <div class=""section-title"">User & Game Activity</div>
    <table class=""stat-grid"">
      <tr>
        <td class=""stat-label"">Total User Squads</td>
        <td class=""stat-value"">{TotalUserSquads:N0}</td>
      </tr>
      <tr>
        <td class=""stat-label"">Total Game Records</td>
        <td class=""stat-value"">{TotalGameRecords:N0}</td>
      </tr>
      <tr>
        <td class=""stat-label"">Total Events</td>
        <td class=""stat-value"">{TotalEvents:N0}</td>
      </tr>
      <tr>
        <td class=""stat-label"">Latest Event Date</td>
        <td class=""stat-value"">{(LatestEventDate.HasValue ? LatestEventDate.Value.ToString("yyyy-MM-dd HH:mm:ss") + " UTC" : "None")}</td>
      </tr>
      <tr>
        <td class=""stat-label"">Total Feedbacks</td>
        <td class=""stat-value"">{TotalFeedbacks:N0}</td>
      </tr>
    </table>

    <div class=""section-title"">AI & Data Quality</div>
    <table class=""stat-grid"">
      <tr>
        <td class=""stat-label"">AI Fixtures</td>
        <td class=""stat-value"">{AIFixtureCount:N0}</td>
      </tr>
      <tr>
        <td class=""stat-label"">Players Missing Photos</td>
        <td class=""stat-value""><span class=""badge {(PlayersMissingPhotos > 0 ? "badge-warning" : "badge-neutral")}"">{PlayersMissingPhotos:N0}</span></td>
      </tr>
    </table>

    <div class=""section-title"">Storage & Azure Blobs</div>
    <table class=""stat-grid"">
      <tr>
        <td class=""stat-label"">Total Team Records</td>
        <td class=""stat-value"">{TotalTeamRecords:N0}</td>
      </tr>
      <tr>
        <td class=""stat-label"">Total Single Fixture Records</td>
        <td class=""stat-value"">{TotalSingleFixtureRecords:N0}</td>
      </tr>
    </table>
  </div>
  <div class=""footer"">
    Generated automatically by Squad Functions
  </div>
</div>
</body>
</html>";
    }
}