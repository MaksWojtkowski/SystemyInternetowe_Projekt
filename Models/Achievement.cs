using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.Models;

public class Achievement
{
    public int Id { get; set; }
    public int GameId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    public string? IconUrl { get; set; }
    
    [MaxLength(200)]
    public string Condition { get; set; } = string.Empty;
    
    public Game Game { get; set; } = null!;
    public ICollection<UserAchievement> UserAchievements { get; set; } = [];
}