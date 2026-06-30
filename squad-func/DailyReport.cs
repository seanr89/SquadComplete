using Microsoft.Azure.Functions.Worker;
using squad_func.Services;
using squad_func.Models;
using Microsoft.EntityFrameworkCore;

namespace Squad.Function;

public class DailyReport(SquadContext context, EmailSMTPService emailService, StorageService storageService)
{
    private readonly SquadContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly EmailSMTPService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly StorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

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

        int fixturesMissingTeams = await _context.Fixtures.CountAsync(f => f.HomeTeamId == null ||
            f.AwayTeamId == null);
        int fixturesMissingScores = await _context.Fixtures.CountAsync(f => f.HomeGoalCount == null ||
            f.AwayGoalCount == null);
        int fixturesMissingDates = await _context.Fixtures.CountAsync(f => f.FixtureDate == null);
        var AIFixtureCount = await _context.Fixtures.CountAsync(f => f.FixtureSource == "AI");

        int aiteamCount = await _storageService.GetContainerBlobCount("ai-teams");
        int aiteamSingleCount = await _storageService.GetContainerBlobCount("ai-team-single");

        int feedCount = await _context.Feedback.CountAsync();
        int eventCount = await _context.Events.CountAsync();

        int playerMissingPhotoCount = await _context.Players.CountAsync(p => p.Photo == null || p.Photo == "");


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
            FixturesMissingScores = fixturesMissingScores,
            FixturesMissingDates = fixturesMissingDates,
            TotalTeamRecords = aiteamCount,
            TotalSingleFixtureRecords = aiteamSingleCount,
            AIFixtureCount = AIFixtureCount,
            TotalEvents = eventCount,
            TotalFeedbacks = feedCount,
            PlayersMissingPhotos = playerMissingPhotoCount
        };

        _emailService.SendEmail(
            recipient: "srafferty89@gmail.com",
            subject: "Squad Daily Report",
            body: report.ToString()
        );
    }
}