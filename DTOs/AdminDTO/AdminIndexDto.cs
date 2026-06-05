namespace ArcadeProject.DTOs.AdminDTO;

public class AdminIndexDto
{
    public IReadOnlyList<AdminUserDto> Users { get; init; } = [];
    public IReadOnlyList<AdminGameDto> Games { get; init; } = [];
    public int TotalUsers    { get; init; }
    public int TotalSessions { get; init; }
    public int TotalThreads  { get; init; }
    public int TotalPosts    { get; init; }
}