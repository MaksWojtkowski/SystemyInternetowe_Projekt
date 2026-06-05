namespace ArcadeProject.DTOs.ForumDTO;

public class ThreadDetailDto
{
    public int      ThreadId    { get; init; }
    public string   Title       { get; init; } = string.Empty;
    public string   AuthorId    { get; init; } = string.Empty;
    public bool     IsPinned    { get; init; }
    public DateTime CreatedAt   { get; init; }
    
    public int?     GameId      { get; init; }
    public string?  GameTitle   { get; init; }
    public string?  GameSlug    { get; init; }

    public IReadOnlyList<PostDto> Posts { get; init; } = [];
}