namespace ArcadeProject.Models;

public class UserAchievement
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int AchievementId { get; set; }
    public DateTime UnlockedAt  { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}