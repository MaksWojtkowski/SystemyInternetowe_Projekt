namespace ArcadeProject.DTOs.AdminDTO;

public class AchievementItemDto
{
    public int    Id             { get; init; }
    public string Name           { get; init; } = string.Empty;
    public string Description    { get; init; } = string.Empty;
    public string? IconUrl       { get; init; }
    public string Condition      { get; init; } = string.Empty;
    public int    UnlockedCount  { get; init; }
}