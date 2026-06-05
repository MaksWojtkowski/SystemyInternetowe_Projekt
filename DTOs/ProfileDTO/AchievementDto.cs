namespace ArcadeProject.DTOs.ProfileDTO;

public class AchievementDto
{
    public string  Name        { get; init; } = string.Empty;
    public string  Description { get; init; } = string.Empty;
    public string? IconUrl     { get; init; }
    public DateTime UnlockedAt { get; init; }
}