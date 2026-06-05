using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcadeProject.Data;
using ArcadeProject.DTOs.ProfileDTO;
using ArcadeProject.Models;

namespace ArcadeProject.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext      _db;
    private readonly UserManager<User> _userManager;

    public ProfileController(AppDbContext db, UserManager<User> userManager)
    {
        _db          = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user  = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);

        var sessions = await _db.GameSessions
            .Where(s => s.UserId == user.Id)
            .Include(s => s.Game)
            .OrderByDescending(s => s.PlayedAt)
            .Take(20)
            .ToListAsync();
        var achievements = await _db.UserAchievements
            .Where(ua => ua.UserId == user.Id)
            .Include(ua => ua.Achievement)
            .OrderByDescending(ua => ua.UnlockedAt)
            .ToListAsync();

        var allSessions = await _db.GameSessions
            .Where(s => s.UserId == user.Id)
            .ToListAsync();

        var dto = new ProfileDto
        {
            Username      = user.UserName ?? "",
            Email         = user.Email    ?? "",
            AvatarUrl     = user.AvatarUrl,
            CreatedAt     = user.CreatedAt,
            Role          = roles.FirstOrDefault() ?? "User",
            TotalSessions = allSessions.Count,
            TotalScore    = allSessions
                .GroupBy(s => s.GameId)
                .Sum(g => g.Max(s => s.Score)),
            TotalGames    = allSessions.Select(s => s.GameId).Distinct().Count(),
            RecentSessions = sessions.Select(s => new SessionHistoryDto
            {
                GameTitle = s.Game.Title,
                GameSlug  = s.Game.Slug,
                Score     = s.Score,
                Duration  = s.DurationSeconds,
                PlayedAt  = s.PlayedAt,
            }).ToList(),
            Achievements = achievements.Select(ua => new AchievementDto
            {
                Name        = ua.Achievement.Name,
                Description = ua.Achievement.Description,
                IconUrl     = ua.Achievement.IconUrl,
                UnlockedAt  = ua.UnlockedAt,
            }).ToList(),
        };

        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        return View(new EditProfileDto
        {
            Username  = user.UserName ?? "",
            AvatarUrl = user.AvatarUrl,
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        if (user.UserName != dto.Username)
        {
            var existing = await _userManager.FindByNameAsync(dto.Username);
            if (existing is not null)
            {
                ModelState.AddModelError(nameof(dto.Username), "Ta nazwa jest już zajęta.");
                return View(dto);
            }
        }

        user.UserName  = dto.Username;
        user.AvatarUrl = dto.AvatarUrl;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(dto);
        }

        await _userManager.UpdateSecurityStampAsync(user);

        TempData["Success"] = "Profil zaktualizowany.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordDto());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        var result = await _userManager.ChangePasswordAsync(
            user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(dto);
        }

        TempData["Success"] = "Hasło zostało zmienione.";
        return RedirectToAction(nameof(Index));
    }
}