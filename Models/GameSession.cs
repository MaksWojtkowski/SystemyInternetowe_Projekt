namespace ArcadeProject.Models;

public class GameSession
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public int GameId { get; set; }

    public int Score { get; set; }
    public int DurationSeconds { get; set; } 
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
    public Game Game { get; set; } = null!;
}