namespace ArcadeProject.DTOs;

public class HomeIndexDto
{
    public IReadOnlyList<GameDto> Games         { get; init; } = [];
    public bool   ShowAllGamesLink           { get; init; } = true;
    public int                   TotalGames     { get; init; }
    public int                   TotalSessions  { get; init; }
    public int                   TotalUsers     { get; init; }
    public int                   TotalThreads   { get; init; }
}