using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ArcadeProject.Data;
using ArcadeProject.DTOs.LeaderboardDTO;

namespace ArcadeProject.Controllers;

public class LeaderboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public LeaderboardController(AppDbContext db, IMemoryCache cache)
    {
        _db    = db;
        _cache = cache;
    }
    
    public async Task<IActionResult> Index()
    {
        var dto = await _cache.GetOrCreateAsync("leaderboard:global", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return await BuildLeaderboardAsync();
        }) ?? await BuildLeaderboardAsync();

        return View(dto);
    }

    private async Task<LeaderboardIndexDto> BuildLeaderboardAsync()
    {
        var globalRaw = await _db.GameSessions
            .GroupBy(s => new { s.UserId, s.GameId })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.GameId,
                BestScore = g.Max(s => s.Score),
                GameTitle = g.First().Game.Title,
                Sessions  = g.Count()
            })
            .ToListAsync();
        
        var perPlayer = globalRaw
            .GroupBy(x => x.UserId)
            .Select(g =>
            {
                var best = g.OrderByDescending(x => x.BestScore).First();
                return new
                {
                    UserId        = g.Key,
                    TotalScore    = g.Sum(x => x.BestScore),
                    TotalSessions = g.Sum(x => x.Sessions),
                    BestGameTitle = best.GameTitle,
                    BestGameScore = best.BestScore,
                };
            })
            .OrderByDescending(x => x.TotalScore)
            .ToList();
        
        var userIds   = perPlayer.Select(x => x.UserId).ToList();
        var userNames = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "?");

        var globalEntries = perPlayer
            .Select((x, i) => new GlobalRankEntryDto
            {
                Rank          = i + 1,
                Username      = userNames.GetValueOrDefault(x.UserId, "?"),
                TotalScore    = x.TotalScore,
                TotalGames    = x.TotalSessions,
                BestGameTitle = x.BestGameTitle,
                BestGameScore = x.BestGameScore,
            })
            .ToList();
        
        var games = await _db.Games
            .Where(g => g.IsActive)
            .OrderBy(g => g.Title)
            .ToListAsync();

        var byGame = new List<GameLeaderboardDto>();
        foreach (var game in games)
        {
            var entries = await _db.GameSessions
                .Where(s => s.GameId == game.Id)
                .GroupBy(s => s.UserId)
                .Select(g => new
                {
                    UserId    = g.Key,
                    BestScore = g.Max(s => s.Score),
                    PlayedAt  = g.OrderByDescending(s => s.Score).First().PlayedAt,
                })
                .OrderByDescending(x => x.BestScore)
                .Take(10)
                .ToListAsync();

            var entryUserIds = entries.Select(e => e.UserId).ToList();
            var entryNames   = await _db.Users
                .Where(u => entryUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "?");

            byGame.Add(new GameLeaderboardDto
            {
                GameId  = game.Id,
                Slug    = game.Slug,
                Title   = game.Title,
                Entries = entries.Select((e, i) => new GameRankEntryDto
                {
                    Rank     = i + 1,
                    Username = entryNames.GetValueOrDefault(e.UserId, "?"),
                    Score    = e.BestScore,
                    PlayedAt = e.PlayedAt,
                }).ToList()
            });
        }
        
        var topScore = globalEntries.FirstOrDefault();

        return new LeaderboardIndexDto
        {
            Podium       = globalEntries.Take(3).ToList(),
            Table        = globalEntries.Skip(3).ToList(),
            ByGame       = byGame,
            TotalPlayers  = globalEntries.Count,
            TotalSessions = await _db.GameSessions.CountAsync(),
            HighestScore  = topScore?.BestGameScore ?? 0,
            HighestScoreGame = topScore?.BestGameTitle ?? "-",
        };
    }
}
