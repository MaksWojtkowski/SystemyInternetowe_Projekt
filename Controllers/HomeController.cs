using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcadeProject.Data;
using ArcadeProject.DTOs;

namespace ArcadeProject.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(AppDbContext db, ILogger<HomeController> logger)
    {
        _db    = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var games = await _db.Games
            .Where(g => g.IsActive)
            .OrderBy(g => g.Title)
            .Select(g => new GameDto
            {
                Id            = g.Id,
                Slug          = g.Slug,
                Title         = g.Title,
                Description   = g.Description,
                ThumbnailUrl  = g.ThumbnailUrl,
                TotalSessions = g.GameSessions.Count()
            })
            .ToListAsync();

        var dto = new HomeIndexDto
        {
            Games         = games,
            TotalGames    = games.Count,
            TotalSessions = await _db.GameSessions.CountAsync(),
            TotalUsers    = await _db.Users.CountAsync(),
            TotalThreads  = await _db.Threads.CountAsync(),
        };

        return View(dto);
    }

    public IActionResult Privacy() => View();
}