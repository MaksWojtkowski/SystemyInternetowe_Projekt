namespace ArcadeProject.DTOs.LeaderboardDTO;

public class LeaderboardIndexDto
{
    public IReadOnlyList<GlobalRankEntryDto> Podium  { get; init; } = [];
    
    public IReadOnlyList<GlobalRankEntryDto> Table   { get; init; } = [];
    
    public IReadOnlyList<GameLeaderboardDto> ByGame  { get; init; } = [];
    
    public int TotalPlayers  { get; init; }
    public int TotalSessions { get; init; }
    public int HighestScore  { get; init; }
    public string HighestScoreGame { get; init; } = string.Empty;
}