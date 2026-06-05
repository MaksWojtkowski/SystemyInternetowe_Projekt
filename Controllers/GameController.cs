using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ArcadeProject.Data;
using ArcadeProject.DTOs;

namespace ArcadeProject.Controllers;

public class GameController : Controller
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GameController> _logger;

    // Lista slugów które mają swoje pliki JS/CSS w wwwroot/js/games/ i css/games/
    // Rozszerzaj tę listę wraz z dodawaniem nowych gier
    private static readonly HashSet<string> KnownGames = ["snake", "tetris", "flappy"];

    public GameController(AppDbContext db, IMemoryCache cache, ILogger<GameController> logger)
    {
        _db     = db;
        _cache  = cache;
        _logger = logger;
    }

    // GET /Game  →  lista wszystkich aktywnych gier
    public async Task<IActionResult> Index()
    {
        // Admin widzi wszystkie gry, zwykły użytkownik tylko aktywne
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

    // GET /Game/Play/snake  →  widok konkretnej gry
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

        // Top 10 wyników — cache 60 sekund żeby nie walić w DB przy każdym wejściu
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

        // Nadaj rangi (1, 2, 3…) po pobraniu z cache
        var ranked = topScores
            .Select((e, i) => e with { Rank = i + 1 })
            .ToList();

        // Osobisty rekord zalogowanego gracza
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
        return View(dto);   // zawsze renderuje Views/Game/Play.cshtml
    }

    // POST /api/scores  →  zapis wyniku przez fetch() z JS gry
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

        var session = new TestTest.Models.GameSession
        {
            GameId          = game.Id,
            UserId          = userId,
            Score           = request.Score,
            DurationSeconds = request.DurationSeconds,
            PlayedAt        = DateTime.UtcNow
        };

        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync();

        // Unieważnij cache leaderboardu dla tej gry
        _cache.Remove($"leaderboard:{request.GameSlug}");

        _logger.LogInformation("Score saved: {User} → {Game} = {Score}",
            User.Identity!.Name, request.GameSlug, request.Score);

        return Ok(new { message = "Wynik zapisany!", score = request.Score });
    }
}

/// <summary>Body JSON z fetch() wysyłanego przez JS gry.</summary>
public record SaveScoreRequest(string GameSlug, int Score, int DurationSeconds);
