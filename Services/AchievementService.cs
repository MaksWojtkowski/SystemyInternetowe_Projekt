using Microsoft.EntityFrameworkCore;
using ArcadeProject.Data;
using ArcadeProject.Models;

namespace ArcadeProject.Services;
public class AchievementService
{
    private readonly AppDbContext  _db;
    private readonly IEmailService _email;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(AppDbContext db, IEmailService email,
                               ILogger<AchievementService> logger)
    {
        _db     = db;
        _email  = email;
        _logger = logger;
    }
    
    public async Task<List<Achievement>> CheckAndGrantAsync(
        string userId, int gameId, int score, int durationSeconds)
    {
        var alreadyUnlocked = await _db.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.AchievementId)
            .ToListAsync();
        
        var candidates = await _db.Achievements
            .Where(a => a.GameId == gameId && !alreadyUnlocked.Contains(a.Id))
            .ToListAsync();

        var granted = new List<Achievement>();

        foreach (var achievement in candidates)
        {
            if (EvaluateCondition(achievement.Condition, score, durationSeconds))
            {
                _db.UserAchievements.Add(new UserAchievement
                {
                    UserId        = userId,
                    AchievementId = achievement.Id,
                    UnlockedAt    = DateTime.UtcNow,
                });
                granted.Add(achievement);
                _logger.LogInformation(
                    "Achievement: {User} odblokował '{Achievement}'",
                    userId, achievement.Name);
            }
        }

        if (granted.Any())
            await _db.SaveChangesAsync();

        return granted;
    }
    
    private static bool EvaluateCondition(string condition, int score, int duration)
    {
        try
        {
            if (condition.StartsWith("score>="))
                return score >= int.Parse(condition[7..]);
            if (condition.StartsWith("score>"))
                return score > int.Parse(condition[6..]);
            if (condition.StartsWith("duration>="))
                return duration >= int.Parse(condition[10..]);
            if (condition.StartsWith("duration>"))
                return duration > int.Parse(condition[9..]);
        }
        catch {}

        return false;
    }
}
