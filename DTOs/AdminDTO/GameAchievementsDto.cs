namespace ArcadeProject.DTOs.AdminDTO;

public class GameAchievementsDto
{
    public int    GameId    { get; init; }
    public string GameTitle { get; init; } = string.Empty;
    public IReadOnlyList<AchievementItemDto> Achievements { get; init; } = [];
}