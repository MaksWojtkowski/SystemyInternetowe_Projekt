namespace ArcadeProject.DTOs.AdminDTO;

public class AdminUserDto
{
    public string   Id          { get; init; } = string.Empty;
    public string   Username    { get; init; } = string.Empty;
    public string   Email       { get; init; } = string.Empty;
    public string   Role        { get; init; } = "User";
    public bool     IsLocked    { get; init; }
    public DateTime CreatedAt   { get; init; }
    public int      SessionCount { get; init; }
}