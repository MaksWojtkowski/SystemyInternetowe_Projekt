using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.Models;

public class Game
{
    public int Id { get; set; }
    
    [Required, MaxLength(50)]
    public string Slug { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<GameSession> GameSessions { get; set; } = [];
    public ICollection<Achievement> Achievements { get; set; } = [];
    public ICollection<Thread> Threads { get; set; } = [];
}