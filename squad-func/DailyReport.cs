using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using squad_func.Services;
using squad_func.Models;
using Microsoft.EntityFrameworkCore;

namespace Squad.Function;

public class DailyReport(ILoggerFactory loggerFactory, SquadContext context, EmailSMTPService emailService)
{
    private readonly ILogger _logger;
    private readonly SquadContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly EmailSMTPService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

    [Function("DailyReport")]
    public async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer)
    {
        var date = DateTime.Now.AddDays(-1);
        var teamCount = await _context.Teams.CountAsync();
        var activeTeamCount = await _context.Teams.CountAsync(t => t.Active);
        var memberCount = await _context.Players.CountAsync();
        var fixtureCount = await _context.Fixtures.CountAsync();
        var gameRecordCount = await _context.GameRecords.CountAsync();
        var userGameRecordCount = await _context.UserSquads.CountAsync();

        int fixturesMissingTeams = await _context.Fixtures.CountAsync(f => f.HomeTeamId == null || f.AwayTeamId == null);
        int fixturesMissingScores = await _context.Fixtures.CountAsync(f => f.HomeGoalCount == null || f.AwayGoalCount == null);

        var report = new DailyStats
        {
            Date = date,
            TotalTeams = teamCount,
            ActiveTeams = activeTeamCount,
            TotalPlayers = memberCount,
            TotalMatches = fixtureCount,
            TotalGameRecords = gameRecordCount,
            TotalUserSquads = userGameRecordCount,
            FixturesMissingTeams = fixturesMissingTeams,
            FixturesMissingScores = fixturesMissingScores
        };

        _emailService.SendEmail(
            recipient: "srafferty89@gmail.com",
            subject: "Squad Daily Report",
            body: report.ToString()
        );
    }
}