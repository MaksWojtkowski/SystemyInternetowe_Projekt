namespace ArcadeProject.DTOs.LeaderboardDTO;

public class GameRankEntryDto
{
    public int      Rank      { get; init; }
    public string   Username  { get; init; } = string.Empty;
    public int      Score     { get; init; }
    public DateTime PlayedAt  { get; init; }
}