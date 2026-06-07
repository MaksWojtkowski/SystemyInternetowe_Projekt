using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcadeProject.Data;
using ArcadeProject.DTOs.AdminDTO;
using ArcadeProject.Models;

namespace ArcadeProject.Controllers;

[Authorize(Roles = "Admin")]  
public class AdminController : Controller
{
    private readonly AppDbContext      _db;
    private readonly UserManager<User> _userManager;

    public AdminController(AppDbContext db, UserManager<User> userManager)
    {
        _db          = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .ToListAsync();

        var userDtos = new List<AdminUserDto>();
        foreach (var u in users)
        {
            var roles    = await _userManager.GetRolesAsync(u);
            var lockout  = await _userManager.IsLockedOutAsync(u);
            var sessions = await _db.GameSessions.CountAsync(s => s.UserId == u.Id);
            userDtos.Add(new AdminUserDto
            {
                Id           = u.Id,
                Username     = u.UserName ?? "",
                Email        = u.Email    ?? "",
                Role         = roles.FirstOrDefault() ?? "User",
                IsLocked     = lockout,
                CreatedAt    = u.CreatedAt,
                SessionCount = sessions,
            });
        }

        var games = await _db.Games
            .OrderBy(g => g.Title)
            .Select(g => new AdminGameDto
            {
                Id           = g.Id,
                Slug         = g.Slug,
                Title        = g.Title,
                Description  = g.Description,
                ThumbnailUrl = g.ThumbnailUrl,
                IsActive     = g.IsActive,
                SessionCount = g.GameSessions.Count(),
            })
            .ToListAsync();

        var recentThreads = await _db.Threads
            .OrderByDescending(t => t.CreatedAt)
            .Take(50)
            .Select(t => new ThreadSummaryDto
            {
                Id         = t.Id,
                Title      = t.Title,
                AuthorName = t.User.UserName ?? "?",
                PostCount  = t.Posts.Count(),
                CreatedAt  = t.CreatedAt,
            })
            .ToListAsync();

        return View(new AdminIndexDto
        {
            Users         = userDtos,
            Games         = games,
            RecentThreads = recentThreads,
            TotalUsers    = userDtos.Count,
            TotalSessions = await _db.GameSessions.CountAsync(),
            TotalThreads  = await _db.Threads.CountAsync(),
            TotalPosts    = await _db.Posts.CountAsync(),
        });
    }

    // ── ZARZĄDZANIE UŻYTKOWNIKAMI ─────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string userId, string role)
    {
        if (role != "Admin" && role != "User")
        {
            TempData["Error"] = "Nieprawidłowa rola.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            TempData["Error"] = "Użytkownik nie istnieje.";
            return RedirectToAction(nameof(Index));
        }

        if (user.Id == _userManager.GetUserId(User) && role == "User")
        {
            TempData["Error"] = "Nie możesz odebrać sobie roli admina.";
            return RedirectToAction(nameof(Index));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = $"Rola użytkownika {user.UserName} zmieniona na {role}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            TempData["Error"] = "Użytkownik nie istnieje.";
            return RedirectToAction(nameof(Index));
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Nie możesz zablokować własnego konta.";
            return RedirectToAction(nameof(Index));
        }

        var isLocked = await _userManager.IsLockedOutAsync(user);
        if (isLocked)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            TempData["Success"] = $"Konto {user.UserName} odblokowane.";
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            TempData["Success"] = $"Konto {user.UserName} zablokowane.";
        }

        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int postId)
    {
        var post = await _db.Posts.FindAsync(postId);
        if (post is null)
        {
            TempData["Error"] = "Post nie istnieje.";
            return RedirectToAction(nameof(Index));
        }

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Post usunięty.";
        return RedirectToAction(nameof(Index));
    }

    // ── ZARZĄDZANIE GRAMI ─────────────────────────────────────────────────────

     [HttpGet]
    public async Task<IActionResult> GameForm(int? id)
    {
        if (id is null) return View(new UpsertGameDto());

        var game = await _db.Games.FindAsync(id);
        if (game is null) return NotFound();

        return View(new UpsertGameDto
        {
            Id           = game.Id,
            Slug         = game.Slug,
            Title        = game.Title,
            Description  = game.Description,
            ThumbnailUrl = game.ThumbnailUrl,
            IsActive     = game.IsActive,
        });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GameForm(UpsertGameDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        if (dto.Id is null)
        {
            var game = new Game
            {
                Slug         = dto.Slug,
                Title        = dto.Title,
                Description  = dto.Description,
                ThumbnailUrl = dto.ThumbnailUrl,
                IsActive     = dto.IsActive,
                CreatedAt    = DateTime.UtcNow,
            };
            _db.Games.Add(game);
            TempData["Success"] = $"Gra \"{dto.Title}\" dodana.";
        }
        else
        {
            var game = await _db.Games.FindAsync(dto.Id);
            if (game is null) return NotFound();

            game.Slug         = dto.Slug;
            game.Title        = dto.Title;
            game.Description  = dto.Description;
            game.ThumbnailUrl = dto.ThumbnailUrl;
            game.IsActive     = dto.IsActive;
            TempData["Success"] = $"Gra \"{dto.Title}\" zaktualizowana.";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleGame(int id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game is null) return NotFound();

        game.IsActive = !game.IsActive;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Gra \"{game.Title}\" {(game.IsActive ? "włączona" : "wyłączona")}.";
        return RedirectToAction(nameof(Index));
    }
    
    // ── ZARZĄDZANIE ACHIEVEMENTAMI ────────────────────────────────────────────
    
    [HttpGet]
    public async Task<IActionResult> Achievements(int gameId)
    {
        var game = await _db.Games.FindAsync(gameId);
        if (game is null) return NotFound();

        var achievements = await _db.Achievements
            .Where(a => a.GameId == gameId)
            .Select(a => new AchievementItemDto
            {
                Id            = a.Id,
                Name          = a.Name,
                Description   = a.Description,
                IconUrl       = a.IconUrl,
                Condition     = a.Condition,
                UnlockedCount = a.UserAchievements.Count(),
            })
            .ToListAsync();

        return View(new GameAchievementsDto
        {
            GameId       = game.Id,
            GameTitle    = game.Title,
            Achievements = achievements,
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> AchievementForm(int gameId, int? id)
    {
        var game = await _db.Games.FindAsync(gameId);
        if (game is null) return NotFound();

        ViewBag.GameTitle = game.Title;

        if (id is null)
            return View(new UpsertAchievementDto { GameId = gameId });

        var achievement = await _db.Achievements.FindAsync(id);
        if (achievement is null) return NotFound();

        return View(new UpsertAchievementDto
        {
            Id          = achievement.Id,
            GameId      = achievement.GameId,
            Name        = achievement.Name,
            Description = achievement.Description,
            IconUrl     = achievement.IconUrl,
            Condition   = achievement.Condition,
        });
    }
    
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AchievementForm(UpsertAchievementDto dto)
    {
        var game = await _db.Games.FindAsync(dto.GameId);
        if (game is null) return NotFound();

        ViewBag.GameTitle = game.Title;

        if (!ModelState.IsValid) return View(dto);

        if (dto.Id is null)
        {
            _db.Achievements.Add(new ArcadeProject.Models.Achievement
            {
                GameId      = dto.GameId,
                Name        = dto.Name,
                Description = dto.Description,
                IconUrl     = dto.IconUrl,
                Condition   = dto.Condition,
            });
            TempData["Success"] = $"Odznaka \"{dto.Name}\" dodana.";
        }
        else
        {
            var achievement = await _db.Achievements.FindAsync(dto.Id);
            if (achievement is null) return NotFound();

            achievement.Name        = dto.Name;
            achievement.Description = dto.Description;
            achievement.IconUrl     = dto.IconUrl;
            achievement.Condition   = dto.Condition;
            TempData["Success"] = $"Odznaka \"{dto.Name}\" zaktualizowana.";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Achievements), new { gameId = dto.GameId });
    }
    
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAchievement(int id)
    {
        var achievement = await _db.Achievements.FindAsync(id);
        if (achievement is null) return NotFound();

        var gameId = achievement.GameId;
        _db.Achievements.Remove(achievement);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Odznaka \"{achievement.Name}\" usunięta.";
        return RedirectToAction(nameof(Achievements), new { gameId });
    }
    
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            TempData["Error"] = "Użytkownik nie istnieje.";
            return RedirectToAction(nameof(Index));
        }
        
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Nie możesz usunąć własnego konta.";
            return RedirectToAction(nameof(Index));
        }
        
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = $"Nie można usunąć konta admina ({user.UserName}). Najpierw zmień mu rolę na User.";
            return RedirectToAction(nameof(Index));
        }

        var username = user.UserName ?? userId;
        
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            TempData["Error"] = $"Błąd podczas usuwania konta: {errors}";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"Konto użytkownika \"{username}\" zostało usunięte.";
        return RedirectToAction(nameof(Index));
    }

    // ── USUWANIE POSTÓW FORUM ─────────────────────────────────────────────────
    
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int postId, int? threadId = null)
    {
        var post = await _db.Posts
            .Include(p => p.Thread)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post is null)
        {
            TempData["Error"] = "Post nie istnieje.";
            return RedirectToAction(nameof(Index));
        }

        var returnThreadId = threadId ?? post.ThreadId;

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Post został usunięty.";
        
        if (Request.Headers["Referer"].ToString().Contains("/Forum/"))
            return Redirect(Url.Action("Thread", "Forum", new { id = returnThreadId })!);

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteThread(int threadId)
    {
        var thread = await _db.Threads
            .Include(t => t.Posts)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread is null)
        {
            TempData["Error"] = "Wątek nie istnieje.";
            return RedirectToAction(nameof(Index));
        }

        var title = thread.Title;
        _db.Threads.Remove(thread);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Wątek \"{title}\" i wszystkie jego posty zostały usunięte.";
        return RedirectToAction(nameof(Index));
    }
}
