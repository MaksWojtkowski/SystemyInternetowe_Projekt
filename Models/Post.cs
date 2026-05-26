using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.Models;

public class Post
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } 
    
    public Thread Thread { get; set; } = null!;
    public User User { get; set; } = null!;
}