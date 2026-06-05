namespace ArcadeProject.DTOs.ProfileDTO;

public class ProfileDto
{
    public string   Username      { get; init; } = string.Empty;
    public string   Email         { get; init; } = string.Empty;
    public string?  AvatarUrl     { get; init; }
    public DateTime CreatedAt     { get; init; }
    public string   Role          { get; init; } = "User";
    
    public int      TotalSessions { get; init; }
    public int      TotalScore    { get; init; }
    public int      TotalGames    { get; init; }
    
    public IReadOnlyList<SessionHistoryDto> RecentSessions { get; init; } = [];
    
    public IReadOnlyList<AchievementDto> Achievements { get; init; } = [];
}