using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcadeProject.Data;
using ArcadeProject.DTOs;
using ArcadeProject.Models;

namespace ArcadeProject.Controllers;

public class ForumController : Controller
{
    private readonly AppDbContext       _db;
    private readonly UserManager<User>  _userManager;
    private readonly ILogger<ForumController> _logger;

    public ForumController(AppDbContext db, UserManager<User> userManager,
                           ILogger<ForumController> logger)
    {
        _db          = db;
        _userManager = userManager;
        _logger      = logger;
    }

    // ── INDEX — lista wątków (opcjonalnie filtrowana per gra) ─────────────────
    public async Task<IActionResult> Index(int? gameId)
    {
        var query = _db.Threads
            .Include(t => t.User)
            .Include(t => t.Game)
            .Include(t => t.Posts)
            .AsQueryable();

        if (gameId.HasValue)
            query = query.Where(t => t.GameId == gameId);

        var threads = await query
            .OrderByDescending(t => t.IsPinned)
            .ThenByDescending(t => t.Posts.Any()
                ? t.Posts.Max(p => p.CreatedAt)
                : t.CreatedAt)
            .Select(t => new ThreadListItemDto
            {
                Id          = t.Id,
                Title       = t.Title,
                AuthorName  = t.User.UserName ?? "?",
                PostCount   = t.Posts.Count,
                CreatedAt   = t.CreatedAt,
                LastPostAt  = t.Posts.Any()
                    ? t.Posts.Max(p => p.CreatedAt)
                    : t.CreatedAt,
                IsPinned    = t.IsPinned,
                GameId      = t.GameId,
                GameTitle   = t.Game != null ? t.Game.Title : null,
                GameSlug    = t.Game != null ? t.Game.Slug  : null,
            })
            .ToListAsync();

        var games = await _db.Games
            .Where(g => g.IsActive)
            .OrderBy(g => g.Title)
            .Select(g => new GameDto
            {
                Id    = g.Id,
                Slug  = g.Slug,
                Title = g.Title,
            })
            .ToListAsync();

        string? filterTitle = null;
        if (gameId.HasValue)
            filterTitle = games.FirstOrDefault(g => g.Id == gameId)?.Title;

        var dto = new ForumIndexDto
        {
            Threads         = threads,
            Games           = games,
            FilterGameId    = gameId,
            FilterGameTitle = filterTitle,
        };

        return View(dto);
    }

    // ── THREAD — widok wątku z postami ────────────────────────────────────────
    public async Task<IActionResult> Thread(int id)
    {
        var thread = await _db.Threads
            .Include(t => t.Game)
            .Include(t => t.User)
            .Where(t => t.Id == id)
            .Select(t => new ThreadDetailDto
            {
                ThreadId  = t.Id,
                Title     = t.Title,
                AuthorId  = t.UserId,
                IsPinned  = t.IsPinned,
                CreatedAt = t.CreatedAt,
                GameId    = t.GameId,
                GameTitle = t.Game != null ? t.Game.Title : null,
                GameSlug  = t.Game != null ? t.Game.Slug  : null,
                Posts     = t.Posts
                    .OrderBy(p => p.CreatedAt)
                    .Select(p => new PostDto
                    {
                        Id         = p.Id,
                        AuthorName = p.User.UserName ?? "?",
                        AuthorId   = p.UserId,
                        Body       = p.Body,
                        CreatedAt  = p.CreatedAt,
                        UpdatedAt  = p.UpdatedAt,
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (thread is null)
        {
            TempData["Error"] = "Wątek nie istnieje.";
            return RedirectToAction(nameof(Index));
        }

        return View(thread);
    }

    // ── CREATE THREAD — formularz ─────────────────────────────────────────────

    [Authorize]
    public async Task<IActionResult> Create(int? gameId)
    {
        var games = await _db.Games
            .Where(g => g.IsActive)
            .OrderBy(g => g.Title)
            .Select(g => new GameDto { Id = g.Id, Slug = g.Slug, Title = g.Title })
            .ToListAsync();

        ViewBag.Games  = games;
        ViewBag.GameId = gameId;
        return View(new CreateThreadDto { GameId = gameId });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateThreadDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Games  = await _db.Games.Where(g => g.IsActive)
                .Select(g => new GameDto { Id = g.Id, Slug = g.Slug, Title = g.Title })
                .ToListAsync();
            ViewBag.GameId = dto.GameId;
            return View(dto);
        }

        var user   = await _userManager.GetUserAsync(User);
        var thread = new Models.Thread
        {
            Title     = dto.Title,
            UserId    = user!.Id,
            GameId    = dto.GameId,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Threads.Add(thread);

        var firstPost = new Post
        {
            Thread    = thread,
            UserId    = user.Id,
            Body      = dto.Body,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Posts.Add(firstPost);

        await _db.SaveChangesAsync();
        _logger.LogInformation("Nowy wątek #{Id} od {User}", thread.Id, user.UserName);

        return RedirectToAction(nameof(Thread), new { id = thread.Id });
    }

    // ── ADD POST — odpowiedź w wątku ─────────────────────────────────────────

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPost(CreatePostDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Treść posta jest wymagana (min. 2 znaki).";
            return RedirectToAction(nameof(Thread), new { id = dto.ThreadId });
        }

        var thread = await _db.Threads.FindAsync(dto.ThreadId);
        if (thread is null)
        {
            TempData["Error"] = "Wątek nie istnieje.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        var post = new Post
        {
            ThreadId  = dto.ThreadId,
            UserId    = user!.Id,
            Body      = dto.Body,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();


        return Redirect(Url.Action(nameof(Thread), new { id = post.ThreadId })
                        + $"#post-{post.Id}");
    }

    // ── EDIT POST — formularz edycji ─────────────────────────────────────────
    [Authorize]
    public async Task<IActionResult> EditPost(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (post.UserId != user!.Id)
        {
            TempData["Error"] = "Możesz edytować tylko własne posty.";
            return RedirectToAction(nameof(Thread), new { id = post.ThreadId });
        }

        return View(new EditPostDto { PostId = post.Id, Body = post.Body });
    }
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(EditPostDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var post = await _db.Posts.FindAsync(dto.PostId);
        if (post is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (post.UserId != user!.Id)
        {
            TempData["Error"] = "Możesz edytować tylko własne posty.";
            return RedirectToAction(nameof(Thread), new { id = post.ThreadId });
        }

        post.Body      = dto.Body;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Redirect(Url.Action(nameof(Thread), new { id = post.ThreadId })
                        + $"#post-{post.Id}");
    }

    // ── DELETE POST ───────────────────────────────────────────────────────────
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _db.Posts
            .Include(p => p.Thread)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (post.UserId != user!.Id)
        {
            TempData["Error"] = "Możesz usuwać tylko własne posty.";
            return RedirectToAction(nameof(Thread), new { id = post.ThreadId });
        }

        var threadId = post.ThreadId;
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Post został usunięty.";
        return RedirectToAction(nameof(Thread), new { id = threadId });
    }
}