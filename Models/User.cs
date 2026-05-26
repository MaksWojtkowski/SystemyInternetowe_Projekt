using Microsoft.AspNetCore.Identity;

namespace ArcadeProject.Models;

public class User : IdentityUser
{
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<GameSession> GameSessions { get; set; } = [];
    public ICollection<UserAchievement> UserAchievements { get; set; } = [];
    public ICollection<Thread> Threads { get; set; } = [];
    public ICollection<Post> Posts { get; set; } = [];
}