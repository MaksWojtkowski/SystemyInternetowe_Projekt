namespace ArcadeProject.DTOs.ForumDTO;

public class ForumIndexDto
{
    public IReadOnlyList<ThreadListItemDto> Threads    { get; init; } = [];
    public IReadOnlyList<GameDto>           Games      { get; init; } = [];
    public int?                             FilterGameId { get; init; }
    public string?                          FilterGameTitle { get; init; }
}