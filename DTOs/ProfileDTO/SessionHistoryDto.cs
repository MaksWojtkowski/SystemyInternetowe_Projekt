namespace ArcadeProject.DTOs.ProfileDTO;

public class SessionHistoryDto
{
    public string   GameTitle { get; init; } = string.Empty;
    public string   GameSlug  { get; init; } = string.Empty;
    public int      Score     { get; init; }
    public int      Duration  { get; init; }
    public DateTime PlayedAt  { get; init; }
}