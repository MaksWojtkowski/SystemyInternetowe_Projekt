using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ArcadeProject.Data;
using ArcadeProject.DTOs;
using ArcadeProject.Services;

namespace ArcadeProject.Controllers;

public class GameController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GameController> _logger;
    private readonly AchievementService _achievements;

    public GameController(AppDbContext db, IMemoryCache cache, ILogger<GameController> logger, AchievementService achievements)
    {
        _db     = db;
        _cache  = cache;
        _logger = logger;
        _achievements = achievements;
    }
    
    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole("Admin");
        
        var games = await _db.Games
            .Where(g => isAdmin || g.IsActive)
            .OrderBy(g => g.Title)
            .Select(g => new GameDto
            {
                Id            = g.Id,
                Slug          = g.Slug,
                Title         = g.Title,
                Description   = g.Description,
                ThumbnailUrl  = g.ThumbnailUrl,
                TotalSessions = g.GameSessions.Count(),
                IsActive      = g.IsActive,
            })
            .ToListAsync();

        return View(games);
    }
    
    public async Task<IActionResult> Play(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return RedirectToAction(nameof(Index));

        slug = slug.ToLowerInvariant();
        
        var isAdmin = User.IsInRole("Admin");

        var game = await _db.Games
            .Where(g => g.Slug == slug && (isAdmin || g.IsActive))
            .Select(g => new { g.Id, g.Slug, g.Title, g.Description })
            .FirstOrDefaultAsync();

        if (game is null)
        {
            TempData["Error"] = $"Gra \"{slug}\" nie istnieje lub jest niedostępna.";
            return RedirectToAction(nameof(Index));
        }
        
        var cacheKey = $"leaderboard:{slug}";
        var topScores = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);

            return await _db.GameSessions
                .Where(s => s.GameId == game.Id)
                .OrderByDescending(s => s.Score)
                .Take(10)
                .Select(s => new LeaderboardEntryDto
                {
                    Rank     = 0,           // nadajemy poniżej
                    Username = s.User.UserName ?? "Anonim",
                    Score    = s.Score,
                    PlayedAt = s.PlayedAt
                })
                .ToListAsync();
        }) ?? [];
        
        var ranked = topScores
            .Select((e, i) => e with { Rank = i + 1 })
            .ToList();
        
        int? personalBest = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _db.Users
                .Where(u => u.UserName == User.Identity.Name)
                .Select(u => u.Id)
                .FirstOrDefault();

            personalBest = await _db.GameSessions
                .Where(s => s.GameId == game.Id && s.UserId == userId)
                .OrderByDescending(s => s.Score)
                .Select(s => (int?)s.Score)
                .FirstOrDefaultAsync();
        }

        var dto = new GamePlayDto
        {
            GameId      = game.Id,
            Slug        = game.Slug,
            Title       = game.Title,
            Description = game.Description,
            IsLoggedIn  = User.Identity?.IsAuthenticated ?? false,
            TopScores   = ranked,
            PersonalBest = personalBest
        };

        _logger.LogInformation("User entered game: {Slug}", slug);
        return View(dto);
    }
    
    [HttpPost("/api/scores")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveScore([FromBody] SaveScoreRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Musisz być zalogowany." });

        var game = await _db.Games
            .Where(g => g.Slug == request.GameSlug && g.IsActive)
            .FirstOrDefaultAsync();

        if (game is null)
            return NotFound(new { error = "Nieznana gra." });

        var userId = _db.Users
            .Where(u => u.UserName == User.Identity!.Name)
            .Select(u => u.Id)
            .FirstOrDefault();

        var session = new ArcadeProject.Models.GameSession
        {
            GameId          = game.Id,
            UserId          = userId,
            Score           = request.Score,
            DurationSeconds = request.DurationSeconds,
            PlayedAt        = DateTime.UtcNow
        };

        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync();
        
        var granted = await _achievements.CheckAndGrantAsync(
            userId, game.Id, request.Score, request.DurationSeconds);
        
        _cache.Remove($"leaderboard:{request.GameSlug}");
        _logger.LogInformation("Score saved: {User} → {Game} = {Score}",
            User.Identity!.Name, request.GameSlug, request.Score);

        return Ok(new
        {
            message      = "Wynik zapisany!",
            score        = request.Score,
            newAchievements = granted.Select(a => new
            {
                name        = a.Name,
                description = a.Description,
                icon        = a.IconUrl
            })
        });
    }
}

public record SaveScoreRequest(string GameSlug, int Score, int DurationSeconds);
