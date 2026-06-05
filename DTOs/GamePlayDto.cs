namespace ArcadeProject.DTOs;

public class GamePlayDto
{
    public int     GameId       { get; init; }
    public string  Slug         { get; init; } = string.Empty;
    public string  Title        { get; init; } = string.Empty;
    public string? Description  { get; init; }
    
    public bool    IsLoggedIn   { get; init; }
    
    public IReadOnlyList<LeaderboardEntryDto> TopScores { get; init; } = [];
    
    public int?    PersonalBest { get; init; }
}