namespace ArcadeProject.DTOs.LeaderboardDTO;

public class GlobalRankEntryDto
{
    public int    Rank         { get; init; }
    public string Username     { get; init; } = string.Empty;
    public int    TotalScore   { get; init; }
    public int    TotalGames   { get; init; }
    public string BestGameTitle { get; init; } = string.Empty;
    public int    BestGameScore { get; init; }
}