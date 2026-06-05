using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ArcadeProject.Data;
using ArcadeProject.DTOs;

namespace ArcadeProject.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<HomeController> _logger;
    private readonly IStringLocalizer<HomeController> _loc;

    public HomeController(AppDbContext db, ILogger<HomeController> logger, IStringLocalizer<HomeController> loc)
    {
        _db    = db;
        _logger = logger;
        _loc    = loc;
    }

    public async Task<IActionResult> Index()
    {
        var isAdmin  = User.IsInRole("Admin");
        var cutoff   = DateTime.UtcNow.AddDays(-30);

        var games = await _db.Games
            .Where(g => isAdmin || g.IsActive)
            .OrderByDescending(g => g.GameSessions
                .Count(s => s.PlayedAt >= cutoff))  // sortuj po sesjach z ostatnich 30 dni
            .ThenBy(g => g.Title)                   // remisy alfabetycznie
            .Take(4)
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

        var dto = new HomeIndexDto
        {
            Games         = games,
            ShowAllGamesLink = true,
            TotalGames    = games.Count,
            TotalSessions = await _db.GameSessions.CountAsync(),
            TotalUsers    = await _db.Users.CountAsync(),
            TotalThreads  = await _db.Threads.CountAsync(),
        };
        
        _logger.LogInformation(_loc["LogLoaded"], dto.TotalGames, dto.TotalSessions);

        return View(dto);
    }

    public IActionResult Privacy() => View();
}