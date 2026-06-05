namespace ArcadeProject.DTOs;

public class GameDto
{
    public int     Id           { get; init; }
    public string  Slug         { get; init; } = string.Empty;
    public string  Title        { get; init; } = string.Empty;
    public string? Description  { get; init; }
    public string? ThumbnailUrl { get; init; }
    public int     TotalSessions { get; init; }
    public bool IsActive { get; set; }
}