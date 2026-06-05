namespace ArcadeProject.DTOs.AdminDTO;

public class AdminGameDto
{
    public int     Id          { get; init; }
    public string  Slug        { get; init; } = string.Empty;
    public string  Title       { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ThumbnailUrl { get; init; }
    public bool    IsActive    { get; init; }
    public int     SessionCount { get; init; }
}