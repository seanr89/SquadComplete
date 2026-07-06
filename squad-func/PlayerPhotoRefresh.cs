using Microsoft.Azure.Functions.Worker;
using squad_func.Models;

using Microsoft.EntityFrameworkCore;

namespace Squad.Function;

public class PlayerPhotoRefresh(SquadContext context, GeminiService geminiService)
{
    private readonly SquadContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly GeminiService _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));

    [Function("PlayerPhotoRefresh")]
    public async Task Run([TimerTrigger("0 0 6 * * *")] TimerInfo myTimer)
    {
        var playersMissingPhotos = await _context.Players.CountAsync(p => p.Photo == null || p.Photo == "");

        if(playersMissingPhotos > 0)
        {
            var players = await _context.Players
                .Where(p => p.Photo == null || p.Photo == "")
                .OrderBy(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            bool hasUpdates = false;
            foreach (var player in players)
            {
                var playerName = player.Name;
                // so lets get the player lastname by space split
                var lastname = playerName.Split(' ').LastOrDefault();
                if (!string.IsNullOrEmpty(lastname))
                {
                    
                }
            }

            if (hasUpdates)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}