using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.Models;

public class Thread
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    
    public int? GameId { get; set; }

    public bool   IsPinned  { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
    public Game? Game { get; set; }
    public ICollection<Post> Posts { get; set; } = [];
}