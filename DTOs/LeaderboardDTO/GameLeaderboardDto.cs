namespace ArcadeProject.DTOs.LeaderboardDTO;

public class GameLeaderboardDto
{
    public int    GameId   { get; init; }
    public string Slug     { get; init; } = string.Empty;
    public string Title    { get; init; } = string.Empty;
    public IReadOnlyList<GameRankEntryDto> Entries { get; init; } = [];
}